// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Threading;
using PaperMalKing.Common;
using PaperMalKing.Database;
using PaperMalKing.Database.Models.MyAnimeList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Favorites;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.AnimeList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.Base;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.MangaList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Types;
using PaperMalKing.UpdatesProviders.Base;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;

namespace PaperMalKing.MyAnimeList.UpdateProvider;

internal sealed class MalUpdateProvider(ILogger<MalUpdateProvider> logger, IOptionsMonitor<MalOptions> _options, IMyAnimeListClient _client, IDbContextFactory<DatabaseContext> _dbContextFactory)
	: BaseUpdateProvider(logger)
{
	protected override TimeSpan DelayBetweenTimerFires => TimeSpan.FromMilliseconds(_options.CurrentValue.DelayBetweenChecksInMilliseconds);

	public override string Name => Constants.Name;

	public override event AsyncEventHandler<UpdateFoundEventArgs>? UpdateFoundEvent;

	[SuppressMessage("Roslynator", "RCS1261:Resource can be disposed asynchronously", Justification = "Sqlite does not support async")]
	protected override async Task CheckForUpdatesAsync(CancellationToken cancellationToken)
	{
		static bool HasUserBeenInactiveRecently(MalUser x)
		{
			const int oneThird = 3;
			var now = TimeProvider.System.GetUtcNow().ToUnixTimeMilliseconds();
			if ((now - Math.Max(x.LastUpdatedAnimeListTimestamp.ToUnixTimeMilliseconds(), x.LastUpdatedMangaListTimestamp.ToUnixTimeMilliseconds())) >
				TimeSpan.FromDays(5).TotalMilliseconds)
			{
				return now % oneThird == 0;
			}

			return false;
		}

		if (this.UpdateFoundEvent is null)
		{
			return;
		}

		using var db = _dbContextFactory.CreateDbContext();

		var users = db.MalUsers.TagWith("Query users for update checking").TagWithCallSite().Where(user => user.DiscordUser.Guilds.Any() &&
			// Is bitwise to allow executing as SQL
			((user.Features & MalUserFeatures.AnimeList) != 0 || (user.Features & MalUserFeatures.MangaList) != 0 ||
			 (user.Features & MalUserFeatures.Favorites) != 0)).OrderBy(_ => EF.Functions.Random()).ToArray();

		foreach (var dbUser in users)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				break;
			}

			if (HasUserBeenInactiveRecently(dbUser))
			{
				this.Logger.SkippingCheckForUser(dbUser.Username,
					dbUser.LastUpdatedAnimeListTimestamp > dbUser.LastUpdatedMangaListTimestamp ? dbUser.LastUpdatedAnimeListTimestamp : dbUser.LastUpdatedMangaListTimestamp);
				continue;
			}

			using var scope = logger.CheckingForUsersUpdatesScope(dbUser.Username);
			this.Logger.StartingToCheckUpdatesFor(dbUser.Username);
			User? user;
			using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			cts.CancelAfter(TimeSpan.FromMinutes(5));
			var perUserCancellationToken = cts.Token;
			const int serverSideErrorHttpCode = 500;
			try
			{
				user = await _client.GetUserAsync(dbUser.Username, dbUser.Features.ToParserOptions(), perUserCancellationToken);
				this.Logger.LoadedProfile(user.Username);
			}
			catch (HttpRequestException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
			{
				this.Logger.UserNotFound(exception, dbUser.Username);
				var username = await _client.GetUsernameAsync(dbUser.UserId, perUserCancellationToken);
				this.Logger.NewUsernameForUser(dbUser.Username, username);
				dbUser.Username = username;
				await db.SaveChangesAndThrowOnNoneAsync(CancellationToken.None);
				continue;
			}
			catch (HttpRequestException exception) when ((int?)exception.StatusCode >= serverSideErrorHttpCode)
			{
				this.Logger.MalServerError(exception);
				return;
			}
			catch (OperationCanceledException) when (perUserCancellationToken.IsCancellationRequested)
			{
				// Ignore we were canceled
				return;
			}
#pragma warning disable CA1031
			// Modify 'CheckForUpdatesAsync' to catch a more specific allowed exception type, or rethrow the exception
			catch (Exception exception)
#pragma warning restore
			{
				this.Logger.EncounteredUnknownError(exception);
				return;
			}

			var isFavoritesHashMismatch = !dbUser.FavoritesIdHash.Equals(HashHelpers.FavoritesHash(user.Favorites.GetFavoriteIdTypesFromFavorites()), StringComparison.Ordinal);

			var animeListUpdates =
				(dbUser.Features.HasFlag(MalUserFeatures.AnimeList) && user.HasPublicAnimeUpdates &&
				 !dbUser.LastAnimeUpdateHash.Equals(user.LatestAnimeUpdateHash, StringComparison.Ordinal))
					? await this
						.CheckLatestListUpdatesAsync<AnimeListEntry, AnimeListType, AnimeFieldsToRequest, AnimeListEntryNode, AnimeListEntryStatus,
							AnimeMediaType, AnimeAiringStatus, AnimeListStatus>(dbUser, user, dbUser.LastUpdatedAnimeListTimestamp, cancellationToken)
					: [];

			var mangaListUpdates =
				(dbUser.Features.HasFlag(MalUserFeatures.MangaList) && user.HasPublicMangaUpdates && !dbUser.LastMangaUpdateHash.Equals(user.LatestMangaUpdateHash, StringComparison.Ordinal))
					? await this
						.CheckLatestListUpdatesAsync<MangaListEntry, MangaListType, MangaFieldsToRequest, MangaListEntryNode, MangaListEntryStatus,
							MangaMediaType, MangaPublishingStatus, MangaListStatus>(dbUser, user, dbUser.LastUpdatedMangaListTimestamp,
							cancellationToken)
					: [];

			if ((dbUser.Features.HasFlag(MalUserFeatures.Favorites) && isFavoritesHashMismatch) ||
				animeListUpdates is not [] || mangaListUpdates is not [])
			{
				db.Entry(dbUser).Reference(u => u.DiscordUser).Load();
				db.Entry(dbUser.DiscordUser).Collection(du => du.Guilds).Load();
				try
				{
					await this.UpdateFoundEvent.InvokeAsync(this, new(new BaseUpdate(this.GetUpdatesAsync(animeListUpdates, mangaListUpdates, dbUser, user, db, cancellationToken)), dbUser.DiscordUser));
				}
				catch (Exception ex)
				{
					this.Logger.ErrorHappenedWhileSendingUpdateOrSavingToDb(ex);
					throw;
				}
			}
			else
			{
				db.Entry(dbUser).State = EntityState.Unchanged;
				this.Logger.NoUpdatesFound(dbUser.Username);
			}
		}
	}

	private async IAsyncEnumerable<DiscordEmbedBuilder> GetUpdatesAsync(IReadOnlyList<DiscordEmbedBuilder> animeListUpdates,
																		IReadOnlyList<DiscordEmbedBuilder> mangaListUpdates,
																		MalUser dbUser, User user, DatabaseContext db,
																		[EnumeratorCancellation] CancellationToken cancellationToken)
	{
		static DiscordEmbedBuilder FormatEmbed(MalUser dbUser, DiscordEmbedBuilder deb)
		{
			if (dbUser.Features.HasFlag(MalUserFeatures.Mention))
			{
				deb.AddField("By", DiscordHelpers.ToDiscordMention(dbUser.DiscordUser.DiscordUserId), inline: true);
			}

			if (dbUser.Features.HasFlag(MalUserFeatures.Website))
			{
				deb.WithMalUpdateProviderFooter();
			}

			return deb;
		}

		var updatesCount = 0;

		var isFavoritesHashMismatch = !dbUser.FavoritesIdHash.Equals(HashHelpers.FavoritesHash(user.Favorites.GetFavoriteIdTypesFromFavorites()), StringComparison.Ordinal);

		var favoritesUpdates = dbUser.Features.HasFlag(MalUserFeatures.Favorites) && isFavoritesHashMismatch
			? this.CheckFavoritesUpdates(dbUser, user, db)
			: ReadOnlyCollection<DiscordEmbedBuilder>.Empty;

		if (favoritesUpdates is not [])
		{
			foreach (var deb in favoritesUpdates)
			{
				yield return FormatEmbed(dbUser, deb);
				updatesCount++;
			}

			await db.SaveChangesAndThrowOnNoneAsync(cancellationToken);

			dbUser.FavoritesIdHash = HashHelpers.FavoritesHash(db.BaseMalFavorites.TagWith("Query favorites to calculate hash").TagWithCallSite()
																 .Where(x => x.UserId == dbUser.UserId).OrderBy(x => x.Id)
																 .ThenBy(x => (byte)x.FavoriteType)
																 .Select(x => new FavoriteIdType(x.Id, (byte)x.FavoriteType)).ToArray());
			db.SaveChanges();
		}

		if (animeListUpdates is not [])
		{
			foreach (var deb in animeListUpdates.OrderBy(x => x.Timestamp))
			{
				yield return FormatEmbed(dbUser, deb);
				dbUser.LastUpdatedAnimeListTimestamp = deb.Timestamp!.Value;
				await db.SaveChangesAndThrowOnNoneAsync(cancellationToken);

				updatesCount++;
			}

			dbUser.LastAnimeUpdateHash = user.LatestAnimeUpdateHash ?? "";

			await db.SaveChangesAndThrowOnNoneAsync(cancellationToken);
		}

		if (mangaListUpdates is not [])
		{
			foreach (var deb in mangaListUpdates)
			{
				yield return FormatEmbed(dbUser, deb);
				dbUser.LastUpdatedMangaListTimestamp = deb.Timestamp!.Value;
				await db.SaveChangesAndThrowOnNoneAsync(cancellationToken);

				updatesCount++;
			}

			dbUser.LastMangaUpdateHash = user.LatestMangaUpdateHash ?? "";

			await db.SaveChangesAndThrowOnNoneAsync(cancellationToken);
		}

		this.Logger.FoundUpdatesForUser(updatesCount, user.Username);
	}

	private async Task<IReadOnlyList<DiscordEmbedBuilder>> CheckLatestListUpdatesAsync<TLe, TL, TRO, TNode, TStatus, TMediaType, TNodeStatus, TListStatus>(
		MalUser dbUser, User user, DateTimeOffset latestUpdateDateTime, CancellationToken ct)
		where TLe : BaseListEntry<TNode, TStatus, TMediaType, TNodeStatus, TListStatus>
		where TL : IListType
		where TRO : unmanaged, Enum
		where TNode : BaseListEntryNode<TMediaType, TNodeStatus>
		where TStatus : BaseListEntryStatus<TListStatus>
		where TMediaType : unmanaged, Enum
		where TNodeStatus : unmanaged, Enum
		where TListStatus : unmanaged, Enum
	{
		var listUpdates = await _client.GetLatestListUpdatesAsync<TLe, TL, TRO, TNode, TStatus, TMediaType, TNodeStatus, TListStatus>(
										user.Username, dbUser.Features.ToRequestOptions<TRO>(), ct);
		if (listUpdates is [])
		{
			return [];
		}

		var result = new List<DiscordEmbedBuilder>();
		foreach (var baseListEntry in listUpdates.Where(x => x.Status.UpdatedAt > latestUpdateDateTime))
		{
			var eb = await baseListEntry
				.ToDiscordEmbedBuilderAsync<TLe, TNode, TStatus, TMediaType, TNodeStatus, TListStatus>(user, _client, dbUser, ct);
			result.Add(eb);
		}

		return result;
	}

	private ReadOnlyCollection<DiscordEmbedBuilder> CheckFavoritesUpdates(MalUser dbUser, User user, DatabaseContext db)
	{
		static IReadOnlyList<DiscordEmbedBuilder> ToDiscordEmbedBuilders<TDbf, TWf>(ILogger<BaseUpdateProvider> logger, DbSet<TDbf> dbSet, IReadOnlyList<TWf> resulting, User user, MalUser dbUser)
			where TDbf : BaseMalFavorite, IEquatable<TDbf>
			where TWf : BaseFavorite
		{
			var withUserQuery = dbSet.Where(x => x.UserId == user.Id).TagWith("Query favorite info for user").TagWithCallSite();
			var dbIds = withUserQuery.Select(x => x.Id).Order().ToArray();
			var sr = resulting.Select(fav => fav.Url.Id).Order().ToArray();
			if (dbIds.AsSpan().SequenceEqual(sr))
			{
				logger.DidntFindAnyFavoritesUpdatesForUser(typeof(TWf).Name, dbUser.Username);
				return [];
			}

			var dbEntries = withUserQuery.ToArray();

			var cResulting = resulting.Select(favorite => favorite.ToDbFavorite<TDbf>(dbUser)).ToArray();
			var (addedValues, removedValues) = dbEntries.GetDifference(cResulting);
			if (addedValues is [] && removedValues is [])
			{
				logger.DidntFindAnyFavoritesUpdatesForUser(typeof(TWf).Name, dbUser.Username);
				return [];
			}

			logger.FoundNewFavoritesRemovedFavorites(addedValues.Count, removedValues.Count, typeof(TWf), dbUser.Username);
			var result = new List<DiscordEmbedBuilder>(addedValues.Count + removedValues.Count);

			for (var i = 0; i < addedValues.Count; i++)
			{
				var fav = cResulting.First(favorite => favorite.Id == addedValues[i].Id);
				var deb = fav.ToDiscordEmbedBuilder(added: true, dbUser);
				deb.WithAuthor(user.Username, user.ProfileUrl, user.AvatarUrl);
				result.Add(deb);
			}

			var toRm = new TDbf[removedValues.Count];
			for (var i = 0; i < removedValues.Count; i++)
			{
				var fav = dbEntries.First(favorite => favorite.Id == removedValues[i].Id);
				toRm[i] = fav;
				var deb = fav.ToDiscordEmbedBuilder(added: false, dbUser);
				deb.WithAuthor(user.Username, user.ProfileUrl, user.AvatarUrl);
				result.Add(deb);
			}

			dbSet.AddRange(addedValues);
			foreach (var t in toRm)
			{
				dbSet.Remove(t);
			}

			return result;
		}

		this.Logger.CheckingFavoritesUpdates(dbUser.Username);

		var list = new List<DiscordEmbedBuilder>();
		list.AddRange(ToDiscordEmbedBuilders(this.Logger, db.MalFavoriteAnimes, user.Favorites.FavoriteAnime, user, dbUser));
		list.AddRange(ToDiscordEmbedBuilders(this.Logger, db.MalFavoriteMangas, user.Favorites.FavoriteManga, user, dbUser));
		list.AddRange(ToDiscordEmbedBuilders(this.Logger, db.MalFavoriteCharacters, user.Favorites.FavoriteCharacters, user, dbUser));
		list.AddRange(ToDiscordEmbedBuilders(this.Logger, db.MalFavoritePersons, user.Favorites.FavoritePeople, user, dbUser));
		list.AddRange(ToDiscordEmbedBuilders(this.Logger, db.MalFavoriteCompanies, user.Favorites.FavoriteCompanies, user, dbUser));

		return new(list.SortByThenBy(f => f.Color.HasValue ? f.Color.Value.Value : DiscordColor.None.Value, f => f.Title));
	}
}