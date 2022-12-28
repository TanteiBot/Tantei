// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaperMalKing.Common;
using PaperMalKing.Database;
using PaperMalKing.Database.Models.MyAnimeList;
using PaperMalKing.MyAnimeList.Wrapper;
using PaperMalKing.MyAnimeList.Wrapper.Models;
using PaperMalKing.MyAnimeList.Wrapper.Models.Favorites;
using PaperMalKing.MyAnimeList.Wrapper.Models.List.Official.AnimeList;
using PaperMalKing.MyAnimeList.Wrapper.Models.List.Official.Base;
using PaperMalKing.MyAnimeList.Wrapper.Models.List.Official.MangaList;
using PaperMalKing.MyAnimeList.Wrapper.Models.List.Types;
using PaperMalKing.UpdatesProviders.Base;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;

namespace PaperMalKing.UpdatesProviders.MyAnimeList;

internal sealed class MalUpdateProvider : BaseUpdateProvider
{
	private readonly MyAnimeListClient _client;
	private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
	private readonly IOptions<MalOptions> _options;

	public override string Name => Constants.Name;

	public MalUpdateProvider(ILogger<MalUpdateProvider> logger, IOptions<MalOptions> options, MyAnimeListClient client,
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

#endregion

		if (this.UpdateFoundEvent is null)
		{
			return;
		}

		using var db = this._dbContextFactory.CreateDbContext();

		var users = db.MalUsers.Where(user => user.DiscordUser.Guilds.Any()).Where(user =>
			// Is bitwise to allow executing on server
			(user.Features & MalUserFeatures.AnimeList) != 0 || (user.Features & MalUserFeatures.MangaList) != 0 ||
			(user.Features & MalUserFeatures.Favorites) != 0).OrderBy(x => EF.Functions.Random()).ToArray();
		foreach (var dbUser in users)
		{
			if (cancellationToken.IsCancellationRequested)
				break;
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
				db.MalUsers.Update(dbUser);
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
				: Array.Empty<DiscordEmbedBuilder>();

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

			var totalUpdates = favoritesUpdates.Concat(animeListUpdates).Concat(mangaListUpdates).OrderBy(b => b.Timestamp ?? DateTimeOffset.MinValue)
											   .ToArray();
			if (!totalUpdates.Any())
			{
				this.Logger.LogDebug("Ended checking updates for {@Username} with no updates found", dbUser.Username);
				continue;
			}

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

			using var transaction = db.Database.BeginTransaction();
			try
			{
				db.MalUsers.Update(dbUser);
				if (db.SaveChanges() <= 0) throw new Exception("Couldn't save update in Db");
				transaction.Commit();
				await this.UpdateFoundEvent.Invoke(new(new BaseUpdate(totalUpdates), this, dbUser.DiscordUser)).ConfigureAwait(false);
				this.Logger.LogDebug("Ended checking updates for {@Username} with {@Updates} updates found", dbUser.Username, totalUpdates.Length);
				if (isFavoritesHashMismatch)
				{
					Expression<Func<IMalFavorite, FavoriteIdType>> Selector(FavoriteType type) => x => new FavoriteIdType(x.Id,(byte) type);
					Expression<Func<IMalFavorite,bool>> predicate = x=>x.UserId == dbUser.UserId;

					var fit = db.MalFavoriteAnimes.Where(predicate).Select(Selector(FavoriteType.Anime)).ToList()
								.AddRangeF(db.MalFavoriteMangas.Where(predicate).Select(Selector(FavoriteType.Manga)))
								.AddRangeF(db.MalFavoriteCharacters.Where(predicate).Select(Selector(FavoriteType.Character)))
								.AddRangeF(db.MalFavoritePersons.Where(predicate).Select(Selector(FavoriteType.Person)))
								.AddRangeF(db.MalFavoriteCompanies.Where(predicate).Select(Selector(FavoriteType.Company)));

					dbUser.FavoritesIdHash = Helpers.FavoritesHash(CollectionsMarshal.AsSpan(fit));
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

	private IReadOnlyList<DiscordEmbedBuilder> CheckFavoritesUpdates(MalUser dbUser, User user, DatabaseContext db)
	{
		IReadOnlyList<DiscordEmbedBuilder> ToDiscordEmbedBuilders<TDbf, TWf>(DbSet<TDbf> dbSet, IReadOnlyList<TWf> resulting, User user,
																			 MalUser dbUser) where TDbf : class, IMalFavorite, IEquatable<TDbf>
																							 where TWf : BaseFavorite
		{
			var withUserQuery = dbSet.Where(x => x.UserId == user.Id);
			var dbIds = withUserQuery.Select(x => x.Id).OrderBy(x => x).ToArray();
			var sr = resulting.Select(fav => fav.Url.Id).OrderBy(i => i).ToArray();
			if ((!dbIds.Any() && !resulting.Any()) || dbIds.SequenceEqual(sr))
			{
				this.Logger.LogTrace("Didn't find any {@Name} updates for {@Username}", typeof(TWf).Name, dbUser.Username);
				return Array.Empty<DiscordEmbedBuilder>();
			}

			var dbEntries = withUserQuery.ToList();

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
		list.Sort((b1, b2) => string.Compare(b1.Title, b2.Title, StringComparison.OrdinalIgnoreCase));
		return list.OrderBy(deb => deb.Color.HasValue ? deb.Color.Value.Value : DiscordColor.None.Value).ThenBy(deb => deb.Title).ToArray();
	}
}