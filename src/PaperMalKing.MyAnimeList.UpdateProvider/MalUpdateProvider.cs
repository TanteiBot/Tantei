// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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

internal sealed class MalUpdateProvider : BaseUpdateProvider
{
	private readonly IMyAnimeListClient _client;
	private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

	public override string Name => Constants.Name;

	public MalUpdateProvider(ILogger<MalUpdateProvider> logger, IOptions<MalOptions> options, IMyAnimeListClient client, IDbContextFactory<DatabaseContext> dbContextFactory)
		: base(logger, TimeSpan.FromMilliseconds(options.Value.DelayBetweenChecksInMilliseconds))
	{
		this._client = client;
		this._dbContextFactory = dbContextFactory;
	}

	public override event UpdateFoundEvent? UpdateFoundEvent;

	protected override async Task CheckForUpdatesAsync(CancellationToken cancellationToken)
	{
		async Task<IReadOnlyList<DiscordEmbedBuilder>>
			CheckLatestListUpdatesAsync<TLe, TL, TRO, TNode, TStatus, TMediaType, TNodeStatus, TListStatus>(
				MalUser dbUser, User user, DateTimeOffset latestUpdateDateTime, Action<MalUser, User, DateTimeOffset> dbUpdateAction, CancellationToken ct)
									where TLe : BaseListEntry<TNode, TStatus, TMediaType, TNodeStatus, TListStatus>
									where TL : IListType
									where TRO : unmanaged, Enum
									where TNode : BaseListEntryNode<TMediaType, TNodeStatus>
									where TStatus : BaseListEntryStatus<TListStatus>
									where TMediaType : unmanaged, Enum
									where TNodeStatus : unmanaged, Enum
									where TListStatus : unmanaged, Enum
		{
			var listUpdates = await this._client
										.GetLatestListUpdatesAsync<TLe, TL, TRO, TNode, TStatus, TMediaType, TNodeStatus, TListStatus>(
											user.Username, dbUser.Features.ToRequestOptions<TRO>(), ct);
			if (listUpdates is [])
			{
				return [];
			}

			var newLatestUpdateTimeStamp = listUpdates.MaxBy(x => x.Status.UpdatedAt)!.Status.UpdatedAt;
			dbUpdateAction(dbUser, user, newLatestUpdateTimeStamp);

			var result = new List<DiscordEmbedBuilder>();
			foreach (var baseListEntry in listUpdates.Where(x => x.Status.UpdatedAt > latestUpdateDateTime))
			{
				var eb = await baseListEntry
							   .ToDiscordEmbedBuilderAsync<TLe, TNode, TStatus, TMediaType, TNodeStatus, TListStatus>(
								   user,
								   dbUser.Features,
								   this._client,
								   cancellationToken);
				result.Add(eb);
			}

			return result;
		}

		static bool FilterInactiveUsers(MalUser x)
		{
			var now = TimeProvider.System.GetUtcNow().ToUnixTimeMilliseconds();
			if ((now - Math.Max(x.LastUpdatedAnimeListTimestamp.ToUnixTimeMilliseconds(), x.LastUpdatedMangaListTimestamp.ToUnixTimeMilliseconds())) >
				TimeSpan.FromDays(5).TotalMilliseconds)
			{
				return now % 3 == 0;
			}

			return false;
		}

		if (this.UpdateFoundEvent is null)
		{
			return;
		}

		await using var db = this._dbContextFactory.CreateDbContext();

		var users = db.MalUsers.TagWith("Query users for update checking").TagWithCallSite().Where(user => user.DiscordUser.Guilds.Any() &&

			// Is bitwise to allow executing on server
			((user.Features & MalUserFeatures.AnimeList) != 0 || (user.Features & MalUserFeatures.MangaList) != 0 ||
			 (user.Features & MalUserFeatures.Favorites) != 0)).OrderBy(_ => EF.Functions.Random()).ToArray();
		foreach (var dbUser in users)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				break;
			}

			if (FilterInactiveUsers(dbUser))
			{
				this.Logger.SkippingCheckForUser(dbUser.Username,
					dbUser.LastUpdatedAnimeListTimestamp > dbUser.LastUpdatedMangaListTimestamp ? dbUser.LastUpdatedAnimeListTimestamp : dbUser.LastUpdatedMangaListTimestamp);
				continue;
			}

			this.Logger.StartingToCheckUpdatesFor(dbUser.Username);
			User? user;
			using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			cts.CancelAfter(TimeSpan.FromMinutes(5));
			var ct = cts.Token;
			try
			{
				user = await this._client.GetUserAsync(dbUser.Username, dbUser.Features.ToParserOptions(), ct);
				this.Logger.LoadedProfile(user.Username);
			}
			catch (HttpRequestException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
			{
				this.Logger.UserNotFound(exception, dbUser.Username);
				var username = await this._client.GetUsernameAsync(dbUser.UserId, ct);
				this.Logger.NewUsernameForUser(dbUser.Username, username);
				dbUser.Username = username;
				await db.SaveChangesAndThrowOnNoneAsync(CancellationToken.None);
				return;
			}
			catch (HttpRequestException exception) when ((int?)exception.StatusCode >= 500)
			{
				this.Logger.MalServerError(exception);
				return;
			}
			#pragma warning disable CA1031
			// Modify 'CheckForUpdatesAsync' to catch a more specific allowed exception type, or rethrow the exception
			catch (Exception exception)
				#pragma warning restore CA1031
			{
				this.Logger.EncounteredUnknownError(exception);
				return;
			}

			var isFavoritesHashMismatch = !string.Equals(
				dbUser.FavoritesIdHash,
				HashHelpers.FavoritesHash(user.Favorites.GetFavoriteIdTypesFromFavorites()),
				StringComparison.Ordinal);
			var favoritesUpdates = dbUser.Features.HasFlag(MalUserFeatures.Favorites) && isFavoritesHashMismatch
				? this.CheckFavoritesUpdates(dbUser, user, db)
				: ReadOnlyCollection<DiscordEmbedBuilder>.Empty;

			var animeListUpdates = dbUser.Features.HasFlag(MalUserFeatures.AnimeList)
				? user.HasPublicAnimeUpdates switch
				{
					true when !string.Equals(dbUser.LastAnimeUpdateHash, user.LatestAnimeUpdateHash, StringComparison.Ordinal) => await
						CheckLatestListUpdatesAsync<AnimeListEntry, AnimeListType, AnimeFieldsToRequest, AnimeListEntryNode, AnimeListEntryStatus,
							AnimeMediaType, AnimeAiringStatus, AnimeListStatus>(
							dbUser,
							user,
							dbUser.LastUpdatedAnimeListTimestamp,
							(u, user, timestamp) =>
							{
								u.LastAnimeUpdateHash = user.LatestAnimeUpdateHash!;
								u.LastUpdatedAnimeListTimestamp = timestamp;
							},
							ct),
					_ => [],
				}
				: [];

			var mangaListUpdates = dbUser.Features.HasFlag(MalUserFeatures.MangaList)
				? user.HasPublicMangaUpdates switch
				{
					true when !string.Equals(dbUser.LastMangaUpdateHash, user.LatestMangaUpdateHash, StringComparison.Ordinal) => await
						CheckLatestListUpdatesAsync<MangaListEntry, MangaListType, MangaFieldsToRequest, MangaListEntryNode, MangaListEntryStatus,
							MangaMediaType, MangaPublishingStatus, MangaListStatus>(
								dbUser,
								user,
								dbUser.LastUpdatedMangaListTimestamp,
								(u, user, timestamp) =>
								{
									u.LastMangaUpdateHash = user.LatestMangaUpdateHash!;
									u.LastUpdatedMangaListTimestamp = timestamp;
								},
								ct),
					_ => [],
				}
				: [];

			var updatesCount = animeListUpdates.Count + mangaListUpdates.Count + favoritesUpdates.Count;
			if (updatesCount == 0)
			{
				db.Entry(dbUser).State = EntityState.Unchanged;
				this.Logger.NoUpdatesFound(dbUser.Username);
				continue;
			}

			var totalUpdates = new List<DiscordEmbedBuilder>(updatesCount);
			totalUpdates.AddRange(animeListUpdates);
			totalUpdates.AddRange(mangaListUpdates);
			totalUpdates.AddRange(favoritesUpdates);
			totalUpdates.SortBy(x => x.Timestamp ?? DateTimeOffset.MinValue);
			db.Entry(dbUser).Reference(u => u.DiscordUser).Load();
			db.Entry(dbUser.DiscordUser).Collection(du => du.Guilds).Load();
			if (dbUser.Features.HasFlag(MalUserFeatures.Mention))
			{
				totalUpdates.ForEach(b => b.AddField("By", DiscordHelpers.ToDiscordMention(dbUser.DiscordUser.DiscordUserId), inline: true));
			}

			if (dbUser.Features.HasFlag(MalUserFeatures.Website))
			{
				totalUpdates.ForEach(b => b.WithMalUpdateProviderFooter());
			}

			if (ct.IsCancellationRequested)
			{
				this.Logger.CancellationRequested();
				db.Entry(dbUser).State = EntityState.Unchanged;
				continue;
			}

			try
			{
				if (db.SaveChanges() <= 0)
				{
					throw new NoChangesSavedException();
				}

				await this.UpdateFoundEvent.Invoke(new(new BaseUpdate(totalUpdates), this, dbUser.DiscordUser));
				this.Logger.FoundUpdatesForUser(totalUpdates.Count, dbUser.Username);
				if (isFavoritesHashMismatch)
				{
					dbUser.FavoritesIdHash = HashHelpers.FavoritesHash(db.BaseMalFavorites.TagWith("Query favorites to calculate hash").TagWithCallSite()
																		 .Where(x => x.UserId == dbUser.UserId).OrderBy(x => x.Id)
																		 .ThenBy(x => (byte)x.FavoriteType)
																		 .Select(x => new FavoriteIdType(x.Id, (byte)x.FavoriteType)).ToArray());
					db.SaveChanges();
				}
			}
			catch (Exception ex)
			{
				this.Logger.ErrorHappenedWhileSendingUpdateOrSavingToDb(ex);
				throw;
			}
		}
	}

	private ReadOnlyCollection<DiscordEmbedBuilder> CheckFavoritesUpdates(MalUser dbUser, User user, DatabaseContext db)
	{
		static IReadOnlyList<DiscordEmbedBuilder> ToDiscordEmbedBuilders<TDbf, TWf>(ILogger<BaseUpdateProvider> logger, DbSet<TDbf> dbSet, IReadOnlyList<TWf> resulting, User user, MalUser dbUser)
			where TDbf : BaseMalFavorite, IEquatable<TDbf>
			where TWf : BaseFavorite
		{
			var withUserQuery = dbSet.Where(x => x.UserId == user.Id).TagWith("Query favorite info for user").TagWithCallSite();
			var dbIds = withUserQuery.Select(x => x.Id).OrderBy(x => x).ToArray();
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
				var deb = fav.ToDiscordEmbedBuilder(added: true);
				deb.WithAuthor(user.Username, user.ProfileUrl, user.AvatarUrl);
				result.Add(deb);
			}

			var toRm = new TDbf[removedValues.Count];
			for (var i = 0; i < removedValues.Count; i++)
			{
				var fav = dbEntries.First(favorite => favorite.Id == removedValues[i].Id);
				toRm[i] = fav;
				var deb = fav.ToDiscordEmbedBuilder(added: false);
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

		var list = new List<DiscordEmbedBuilder>(0);
		list.AddRange(ToDiscordEmbedBuilders(this.Logger, db.MalFavoriteAnimes, user.Favorites.FavoriteAnime, user, dbUser));
		list.AddRange(ToDiscordEmbedBuilders(this.Logger, db.MalFavoriteMangas, user.Favorites.FavoriteManga, user, dbUser));
		list.AddRange(ToDiscordEmbedBuilders(this.Logger, db.MalFavoriteCharacters, user.Favorites.FavoriteCharacters, user, dbUser));
		list.AddRange(ToDiscordEmbedBuilders(this.Logger, db.MalFavoritePersons, user.Favorites.FavoritePeople, user, dbUser));
		list.AddRange(ToDiscordEmbedBuilders(this.Logger, db.MalFavoriteCompanies, user.Favorites.FavoriteCompanies, user, dbUser));

		return new(list.SortByThenBy(f => f.Color.HasValue ? f.Color.Value.Value : DiscordColor.None.Value, f => f.Title));
	}
}