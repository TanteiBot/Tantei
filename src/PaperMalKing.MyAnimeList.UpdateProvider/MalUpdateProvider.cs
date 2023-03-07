// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

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
	private readonly IOptions<MalOptions> _options;

	public override string Name => Constants.Name;

	public MalUpdateProvider(ILogger<MalUpdateProvider> logger, IOptions<MalOptions> options, IMyAnimeListClient client,
							 IDbContextFactory<DatabaseContext> dbContextFactory) : base(logger,
		TimeSpan.FromMilliseconds(options.Value.DelayBetweenChecksInMilliseconds))
	{
		this._options = options;
		this._client = client;
		this._dbContextFactory = dbContextFactory;
	}

	public override event UpdateFoundEvent? UpdateFoundEvent;

	protected override async Task CheckForUpdatesAsync(CancellationToken cancellationToken)
	{
#region LocalFuncs

		static void DbAnimeUpdateAction(MalUser u, User user, DateTimeOffset timestamp)
		{
			u.LastAnimeUpdateHash = user.LatestAnimeUpdateHash!;
			u.LastUpdatedAnimeListTimestamp = timestamp;
		}

		static void DbMangaUpdateAction(MalUser u, User user, DateTimeOffset timestamp)
		{
			u.LastMangaUpdateHash = user.LatestMangaUpdateHash!;
			u.LastUpdatedMangaListTimestamp = timestamp;
		}

		async Task<IReadOnlyList<DiscordEmbedBuilder>>
			CheckLatestListUpdatesAsync<TLe, TL, TRO, TNode, TStatus, TMediaType, TNodeStatus, TListStatus>(
				MalUser dbUser, User user, DateTimeOffset latestUpdateDateTime, Action<MalUser, User, DateTimeOffset> dbUpdateAction,
				CancellationToken ct) where TLe : BaseListEntry<TNode, TStatus, TMediaType, TNodeStatus, TListStatus>
									  where TL : IListType
									  where TRO : unmanaged, Enum
									  where TNode : BaseListEntryNode<TMediaType, TNodeStatus>
									  where TStatus : BaseListEntryStatus<TListStatus>
									  where TMediaType : unmanaged, Enum
									  where TNodeStatus : unmanaged, Enum
									  where TListStatus : unmanaged, Enum
		{
			var listUpdates = await this._client
										.GetLatestListUpdatesAsync<TLe, TL, TRO, TNode, TStatus, TMediaType, TNodeStatus, TListStatus>(user.Username,
											dbUser.Features.ToRequestOptions<TRO>(), ct).ConfigureAwait(false);
			if (listUpdates.Count == 0)
			{
				return Array.Empty<DiscordEmbedBuilder>();
			}

			var newLatestUpdateTimeStamp = listUpdates.MaxBy(x => x.Status.UpdatedAt)!.Status.UpdatedAt;
			dbUpdateAction(dbUser, user, newLatestUpdateTimeStamp);

			return listUpdates.Where(x => x.Status.UpdatedAt > latestUpdateDateTime).Select(x =>
				x.ToDiscordEmbedBuilder<TLe, TNode, TStatus, TMediaType, TNodeStatus, TListStatus>(user, dbUser.Features)).ToArray();
		}

		static bool FilterInactiveUsers(MalUser x)
		{
			var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
			if((now - Math.Max(x.LastUpdatedAnimeListTimestamp.ToUnixTimeMilliseconds(),
				   x.LastUpdatedMangaListTimestamp.ToUnixTimeMilliseconds())) > TimeSpan.FromDays(5).TotalMilliseconds)
			{
				return now % 3 == 0;
			}
			return false;
		}

#endregion LocalFuncs

		if (this.UpdateFoundEvent is null)
		{
			return;
		}

		using var db = this._dbContextFactory.CreateDbContext();

		var users = db.MalUsers.Where(user => user.DiscordUser.Guilds.Any()).Where(user =>
						  // Is bitwise to allow executing on server
						  (user.Features & MalUserFeatures.AnimeList) != 0 || (user.Features & MalUserFeatures.MangaList) != 0 ||
						  (user.Features & MalUserFeatures.Favorites) != 0).OrderBy(_ => EF.Functions.Random())
					  .ToArray();
		foreach (var dbUser in users)
		{
			if (cancellationToken.IsCancellationRequested)
				break;
			if (FilterInactiveUsers(dbUser))
			{
				this.Logger.LogDebug("Skipping update check for {@Username}, since last update for user was at {Timestamp}", dbUser.Username,
					dbUser.LastUpdatedAnimeListTimestamp > dbUser.LastUpdatedMangaListTimestamp
						? dbUser.LastUpdatedAnimeListTimestamp
						: dbUser.LastUpdatedMangaListTimestamp);
				continue;
			}
			this.Logger.LogDebug("Starting to check for updates for {@Username}", dbUser.Username);
			User? user = null;
			using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			cts.CancelAfter(TimeSpan.FromMinutes(5));
			var ct = cts.Token;
			try
			{
				user = await this._client.GetUserAsync(dbUser.Username, dbUser.Features.ToParserOptions(), ct).ConfigureAwait(false);
				this.Logger.LogTrace("Loaded profile for {@Username}", user.Username);
			}
			catch (HttpRequestException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
			{
				this.Logger.LogWarning(exception, "User with username {@Username} not found", dbUser.Username);
				var username = await this._client.GetUsernameAsync(dbUser.UserId, ct).ConfigureAwait(false);
				this.Logger.LogInformation("New username for user with {FormerUsername} is {CurrentUsername}", dbUser.Username, username);
				dbUser.Username = username;
				await db.SaveChangesAndThrowOnNoneAsync(CancellationToken.None).ConfigureAwait(false);
				return;
			}
			catch (HttpRequestException exception) when ((int?)exception.StatusCode >= 500)
			{
				this.Logger.LogError(exception, "Mal server encounters some error, skipping current update check");
				return;
			}
			#pragma warning disable CA1031
			catch (Exception exception)
				#pragma warning restore CA1031
			{
				this.Logger.LogError(exception, "Encountered unknown error, skipping current update check");
				return;
			}

			var isFavoritesHashMismatch = dbUser.FavoritesIdHash != Helpers.FavoritesHash(user.Favorites.GetFavoriteIdTypesFromFavorites());
			var favoritesUpdates = dbUser.Features.HasFlag(MalUserFeatures.Favorites) && isFavoritesHashMismatch
				? this.CheckFavoritesUpdates(dbUser, user, db)
				: ReadOnlyCollection<DiscordEmbedBuilder>.Empty;

			var animeListUpdates = dbUser.Features.HasFlag(MalUserFeatures.AnimeList)
				? user.HasPublicAnimeUpdates switch
				{
					true when dbUser.LastAnimeUpdateHash != user.LatestAnimeUpdateHash => await CheckLatestListUpdatesAsync<AnimeListEntry,
						AnimeListType, AnimeFieldsToRequest, AnimeListEntryNode, AnimeListEntryStatus, AnimeMediaType, AnimeAiringStatus,
						AnimeListStatus>(dbUser, user, dbUser.LastUpdatedAnimeListTimestamp, DbAnimeUpdateAction, ct).ConfigureAwait(false),
					_ => Array.Empty<DiscordEmbedBuilder>()
				}
				: Array.Empty<DiscordEmbedBuilder>();

			var mangaListUpdates = dbUser.Features.HasFlag(MalUserFeatures.MangaList)
				? user.HasPublicMangaUpdates switch
				{
					true when dbUser.LastMangaUpdateHash != user.LatestMangaUpdateHash => await CheckLatestListUpdatesAsync<MangaListEntry,
						MangaListType, MangaFieldsToRequest, MangaListEntryNode, MangaListEntryStatus, MangaMediaType, MangaPublishingStatus,
						MangaListStatus>(dbUser, user, dbUser.LastUpdatedMangaListTimestamp, DbMangaUpdateAction, ct).ConfigureAwait(false),
					_ => Array.Empty<DiscordEmbedBuilder>()
				}
				: Array.Empty<DiscordEmbedBuilder>();

			var updatesCount = animeListUpdates.Count + mangaListUpdates.Count + favoritesUpdates.Count;
			if (updatesCount == 0)
			{
				db.Entry(dbUser).State = EntityState.Unchanged;
				this.Logger.LogDebug("Ended checking updates for {@Username} with no updates found", dbUser.Username);
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
				totalUpdates.ForEach(b => b.AddField("By", Helpers.ToDiscordMention(dbUser.DiscordUser.DiscordUserId), true));
			if (dbUser.Features.HasFlag(MalUserFeatures.Website))
				totalUpdates.ForEach(b => b.WithMalUpdateProviderFooter());

			if (ct.IsCancellationRequested)
			{
				this.Logger.LogInformation("Ended checking updates for {@Username} because it was canceled", dbUser.Username);
				db.Entry(dbUser).State = EntityState.Unchanged;
				continue;
			}

			try
			{
				if (db.SaveChanges() <= 0) throw new Exception("Couldn't save update in Db");
				await this.UpdateFoundEvent.Invoke(new(new BaseUpdate(totalUpdates), this, dbUser.DiscordUser)).ConfigureAwait(false);
				this.Logger.LogDebug("Ended checking updates for {@Username} with {@Updates} updates found", dbUser.Username, totalUpdates.Count);
				if (isFavoritesHashMismatch)
				{
					dbUser.FavoritesIdHash = Helpers.FavoritesHash(db.BaseMalFavorites.Where(x => x.UserId == dbUser.UserId).OrderBy(x => x.Id)
																	 .ThenBy(x => (byte)x.FavoriteType)
																	 .Select(x => new FavoriteIdType(x.Id, (byte)x.FavoriteType)).ToArray());
					db.SaveChanges();
				}
			}
			catch (Exception ex)
			{
				this.Logger.LogError(ex, "Error happened while sending update or saving changes to DB");
				throw;
			}
		}
	}

	private ReadOnlyCollection<DiscordEmbedBuilder> CheckFavoritesUpdates(MalUser dbUser, User user, DatabaseContext db)
	{
		IReadOnlyList<DiscordEmbedBuilder> ToDiscordEmbedBuilders<TDbf, TWf>(DbSet<TDbf> dbSet, IReadOnlyList<TWf> resulting, User user,
																			 MalUser dbUser) where TDbf : BaseMalFavorite, IEquatable<TDbf>
																							 where TWf : BaseFavorite
		{
			var withUserQuery = dbSet.Where(x => x.UserId == user.Id);
			var dbIds = withUserQuery.Select(x => x.Id).OrderBy(x => x).ToArray();
			var sr = resulting.Select(fav => fav.Url.Id).OrderBy(i => i).ToArray();
			if ((dbIds.Length == 0 && resulting.Count == 0) || dbIds.SequenceEqual(sr))
			{
				this.Logger.LogTrace("Didn't find any {@Name} updates for {@Username}", typeof(TWf).Name, dbUser.Username);
				return Array.Empty<DiscordEmbedBuilder>();
			}

			var dbEntries = withUserQuery.ToArray();

			var cResulting = resulting.Select(favorite => favorite.ToDbFavorite<TDbf>(dbUser)).ToArray();
			var (addedValues, removedValues) = dbEntries.GetDifference(cResulting);
			if (!addedValues.Any() && !removedValues.Any())
			{
				this.Logger.LogTrace("Didn't find any {@Name} updates for {@Username}", typeof(TWf).Name, dbUser.Username);
				return Array.Empty<DiscordEmbedBuilder>();
			}

			this.Logger.LogTrace("Found {@AddedCount} new favorites, {@RemovedCount} removed favorites of type {@Type} of {@Username}",
				addedValues.Count, removedValues.Count, typeof(TWf), dbUser.Username);

			var result = new List<DiscordEmbedBuilder>(addedValues.Count + removedValues.Count);

			for (var i = 0; i < addedValues.Count; i++)
			{
				var fav = cResulting.First(favorite => favorite.Id == addedValues[i].Id);
				var deb = fav.ToDiscordEmbedBuilder(true);
				deb.WithAuthor(user.Username, user.ProfileUrl, user.AvatarUrl);
				result.Add(deb);
			}

			var toRm = new TDbf[removedValues.Count];
			for (var i = 0; i < removedValues.Count; i++)
			{
				var fav = dbEntries.First(favorite => favorite.Id == removedValues[i].Id);
				toRm[i] = fav;
				var deb = fav.ToDiscordEmbedBuilder(false);
				deb.WithAuthor(user.Username, user.ProfileUrl, user.AvatarUrl);
				result.Add(deb);
			}

			dbSet.AddRange(addedValues);
			foreach (var t in toRm)
				dbSet.Remove(t);

			return result;
		}

		this.Logger.LogTrace("Checking favorites updates of {@Username}", dbUser.Username);

		var list = new List<DiscordEmbedBuilder>(0);
		list.AddRange(ToDiscordEmbedBuilders(db.MalFavoriteAnimes, user.Favorites.FavoriteAnime, user, dbUser));
		list.AddRange(ToDiscordEmbedBuilders(db.MalFavoriteMangas, user.Favorites.FavoriteManga, user, dbUser));
		list.AddRange(ToDiscordEmbedBuilders(db.MalFavoriteCharacters, user.Favorites.FavoriteCharacters, user, dbUser));
		list.AddRange(ToDiscordEmbedBuilders(db.MalFavoritePersons, user.Favorites.FavoritePeople, user, dbUser));
		list.AddRange(ToDiscordEmbedBuilders(db.MalFavoriteCompanies, user.Favorites.FavoriteCompanies, user, dbUser));
		
		return new (list.SortByThenBy(f => f.Color.HasValue ? f.Color.Value.Value : DiscordColor.None.Value, f => f.Title));
	}
}