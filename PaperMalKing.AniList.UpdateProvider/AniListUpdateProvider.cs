﻿// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using GraphQL.Client.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaperMalKing.AniList.UpdateProvider.CombinedResponses;
using PaperMalKing.AniList.Wrapper;
using PaperMalKing.AniList.Wrapper.Models;
using PaperMalKing.AniList.Wrapper.Models.Enums;
using PaperMalKing.AniList.Wrapper.Models.Interfaces;
using PaperMalKing.Common;
using PaperMalKing.Database;
using PaperMalKing.Database.Models.AniList;
using PaperMalKing.UpdatesProviders.Base;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;
using FavouriteType = PaperMalKing.Database.Models.AniList.FavouriteType;

namespace PaperMalKing.AniList.UpdateProvider;

internal sealed class AniListUpdateProvider : BaseUpdateProvider
{
	private readonly AniListClient _client;
	private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

	public AniListUpdateProvider(ILogger<AniListUpdateProvider> logger, IOptions<AniListOptions> options, AniListClient client,
								 IDbContextFactory<DatabaseContext> dbContextFactory) : base(logger,
		TimeSpan.FromMilliseconds(options.Value.DelayBetweenChecksInMilliseconds))
	{
		this._client = client;
		this._dbContextFactory = dbContextFactory;
	}

	public override string Name => Constants.NAME;
	public override event UpdateFoundEvent? UpdateFoundEvent;

	protected override async Task CheckForUpdatesAsync(CancellationToken cancellationToken)
	{
		if (this.UpdateFoundEvent is null)
		{
			return;
		}

		using var db = this._dbContextFactory.CreateDbContext();
		foreach (var dbUser in db.AniListUsers.Where(u =>
					 u.DiscordUser.Guilds.Any() && ((u.Features & AniListUserFeatures.AnimeList) != 0 ||
												    (u.Features & AniListUserFeatures.MangaList) != 0 ||
												    (u.Features & AniListUserFeatures.Favourites) != 0 ||
												    (u.Features & AniListUserFeatures.Reviews) != 0)).OrderBy(x => EF.Functions.Random()).ToArray())
		{
			if (cancellationToken.IsCancellationRequested)
				break;
			using var perUserCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			perUserCancellationTokenSource.CancelAfter(TimeSpan.FromMinutes(3));
			var perUserCancellationToken = perUserCancellationTokenSource.Token;
			this.Logger.LogDebug("Starting to check for updates of {UserId}", dbUser.Id);
			var allUpdates = new List<DiscordEmbedBuilder>();
			CombinedRecentUpdatesResponse recentUserUpdates;
			try
			{
				recentUserUpdates = await this._client.GetAllRecentUserUpdatesAsync(dbUser, dbUser.Features, perUserCancellationToken)
											  .ConfigureAwait(false);
			}
			catch (GraphQLHttpRequestException ex) when (ex.Message.Contains("NotFound", StringComparison.Ordinal))
			{
				continue;
			}

			var isFavouritesHashMismatch = dbUser.FavouritesIdHash !=
										   Helpers.FavoritesHash(recentUserUpdates.Favourites.Select(x => new FavoriteIdType(x.Id, (byte)x.Type))
																			      .OrderBy(x => x.Id).ToArray());

			if ((dbUser.Features & AniListUserFeatures.Favourites) != 0 && isFavouritesHashMismatch)
				allUpdates.AddRange(await this.GetFavouritesUpdatesAsync(recentUserUpdates, dbUser, db, perUserCancellationToken)
											  .ConfigureAwait(false));
			if ((dbUser.Features & AniListUserFeatures.Reviews) != 0)
				allUpdates.AddRange(recentUserUpdates.Reviews.Where(r => r.CreatedAtTimeStamp > dbUser.LastReviewTimestamp)
													 .Select(r => r.ToDiscordEmbedBuilder(recentUserUpdates.User)));
			foreach (var grouping in recentUserUpdates.Activities.GroupBy(activity => activity.Media.Id))
			{
				var lastListActivityOnMedia = grouping.MaxBy(activity => activity.CreatedAtTimestamp)!;
				var mediaListEntry = lastListActivityOnMedia.Media.Type == ListType.ANIME
					? recentUserUpdates.AnimeList.FirstOrDefault(mle => mle.Id == lastListActivityOnMedia.Media.Id)
					: recentUserUpdates.MangaList.FirstOrDefault(mle => mle.Id == lastListActivityOnMedia.Media.Id);
				if (mediaListEntry is not null)
					allUpdates.Add(lastListActivityOnMedia.ToDiscordEmbedBuilder(mediaListEntry, recentUserUpdates.User, dbUser.Features));
			}

			if (!allUpdates.Any())
			{
				dbUser.FavouritesIdHash = Helpers.FavoritesHash(db.AniListFavourites.Where(x => x.UserId == dbUser.Id)
																  .Select(x => new FavoriteIdType(x.Id, (byte)x.FavouriteType)).ToArray());
				db.SaveChanges();

				this.Logger.LogTrace("No updates found for {Username}", recentUserUpdates.User.Name);
				db.Entry(dbUser).State = EntityState.Unchanged;
				continue;
			}

			db.Entry(dbUser).Reference(u => u.DiscordUser).Load();
			db.Entry(dbUser.DiscordUser).Collection(du => du.Guilds).Load();
			var lastActivityTimestamp = recentUserUpdates.Activities.Any() ? recentUserUpdates.Activities.Max(a => a.CreatedAtTimestamp) : 0L;
			var lastReviewTimeStamp = recentUserUpdates.Reviews.Any() ? recentUserUpdates.Reviews.Max(r => r.CreatedAtTimeStamp) : 0L;
			if (dbUser.LastActivityTimestamp < lastActivityTimestamp) dbUser.LastActivityTimestamp = lastActivityTimestamp;
			if (dbUser.LastReviewTimestamp < lastReviewTimeStamp) dbUser.LastReviewTimestamp = lastReviewTimeStamp;
			if ((dbUser.Features & AniListUserFeatures.Mention) != 0)
				allUpdates.ForEach(u => u.AddField("By", Helpers.ToDiscordMention(dbUser.DiscordUserId), true));
			if ((dbUser.Features & AniListUserFeatures.Website) != 0)
				allUpdates.ForEach(u => u.WithAniListFooter());
			allUpdates.Sort((deb1, deb2) => DateTimeOffset.Compare(deb1.Timestamp.GetValueOrDefault(), deb2.Timestamp.GetValueOrDefault()));
			if (perUserCancellationToken.IsCancellationRequested)
			{
				db.Entry(dbUser).State = EntityState.Unchanged;
				this.Logger.LogInformation("Cancellation requested. Stopping");
				break;
			}

			using var transaction = db.Database.BeginTransaction();
			try
			{
				db.Entry(dbUser).State = EntityState.Modified;
				if (db.SaveChanges() <= 0) throw new Exception("Couldn't save updates to database");
				transaction.Commit();
				await this.UpdateFoundEvent.Invoke(new(new BaseUpdate(allUpdates), this, dbUser.DiscordUser)).ConfigureAwait(false);
				if (isFavouritesHashMismatch)
				{
					dbUser.FavouritesIdHash = Helpers.FavoritesHash(db.AniListFavourites.Where(x => x.UserId == dbUser.Id)
																	  .Select(x => new FavoriteIdType(x.Id, (byte)x.FavouriteType)).ToArray());
					db.SaveChanges();
				}
			}
			#pragma warning disable CA1031
			catch (Exception ex)
				#pragma warning restore CA1031
			{
				this.Logger.LogError(ex, "Error happened while sending update or saving changes to DB");
				throw;
			}
		}
	}

	private async Task<IReadOnlyList<DiscordEmbedBuilder>> GetFavouritesUpdatesAsync(CombinedRecentUpdatesResponse response, AniListUser user,
																					 DatabaseContext db, CancellationToken cancellationToken)
	{
		Func<IdentifiableFavourite, AniListFavourite> FavouritesSelector(AniListUser aniListUser)
		{
			return av => new AniListFavourite()
			{
				User = aniListUser,
				UserId = aniListUser.Id,
				Id = av.Id,
				FavouriteType = (FavouriteType)av.Type
			};
		}

		static uint[] GetIds(IReadOnlyList<IdentifiableFavourite> collection, Func<IdentifiableFavourite, bool> predicate)
		{
			return collection.Any(predicate) ? collection.Where(predicate).Select(f => f.Id).ToArray() : Array.Empty<uint>();
		}

		static void GetFavouritesEmbed<T>(List<DiscordEmbedBuilder> aggregator, IReadOnlyList<IdentifiableFavourite> addedValues,
										  IReadOnlyList<IdentifiableFavourite> removedValues, List<T> obtainedValues,
										  Wrapper.Models.Enums.FavouriteType type, User user, AniListUserFeatures features)
			where T : class, IIdentifiable, ISiteUrlable
		{
			foreach (var value in obtainedValues)
			{
				bool? added = null;
				if (addedValues.Any(f => f.Id == value.Id && f.Type == type))
					added = true;
				else if (removedValues.Any(f => f.Id == value.Id && f.Type == type))
					added = false;

				if (added.HasValue)
					aggregator.Add(FavouriteToDiscordEmbedBuilderConverter.Convert(value, user, added.Value, features));
			}
		}

		var withUserQuery = db.AniListFavourites.Where(x => x.UserId == user.Id);
		var convFavs = withUserQuery.Select(f => new IdentifiableFavourite()
		{
			Id = f.Id,
			Type = (Wrapper.Models.Enums.FavouriteType)f.FavouriteType
		}).ToArray();

		var (addedValues, removedValues) = convFavs.GetDifference(response.Favourites);
		if (cancellationToken.IsCancellationRequested || (!addedValues.Any() && !removedValues.Any()))
			return Array.Empty<DiscordEmbedBuilder>();

		db.AniListFavourites.RemoveRange(removedValues.Select(FavouritesSelector(user)));
		db.AniListFavourites.AddRange(addedValues.Select(FavouritesSelector(user)));

		var changedValues = new List<IdentifiableFavourite>(addedValues);
		changedValues.AddRange(removedValues);
		var animeIds = GetIds(changedValues, f => f.Type == Wrapper.Models.Enums.FavouriteType.Anime);
		var mangaIds = GetIds(changedValues, f => f.Type == Wrapper.Models.Enums.FavouriteType.Manga);
		var charIds = GetIds(changedValues, f => f.Type == Wrapper.Models.Enums.FavouriteType.Characters);
		var staffIds = GetIds(changedValues, f => f.Type == Wrapper.Models.Enums.FavouriteType.Staff);
		var studioIds = GetIds(changedValues, f => f.Type == Wrapper.Models.Enums.FavouriteType.Studios);
		var hasNextPage = true;
		var combinedResponse = new CombinedFavouritesInfoResponse();
		var results = new List<DiscordEmbedBuilder>(changedValues.Count);
		for (byte page = 1; hasNextPage; page++)
		{
			var favouritesInfo = await this._client
										   .FavouritesInfoAsync(page, animeIds, mangaIds, charIds, staffIds, studioIds, (RequestOptions)user.Features,
											   cancellationToken).ConfigureAwait(false);
			combinedResponse.Add(favouritesInfo);
			hasNextPage = favouritesInfo.HasNextPage;
		}

		GetFavouritesEmbed(results, addedValues, removedValues, combinedResponse.Anime, Wrapper.Models.Enums.FavouriteType.Anime, response.User,
			user.Features);
		GetFavouritesEmbed(results, addedValues, removedValues, combinedResponse.Manga, Wrapper.Models.Enums.FavouriteType.Manga, response.User,
			user.Features);
		GetFavouritesEmbed(results, addedValues, removedValues, combinedResponse.Characters, Wrapper.Models.Enums.FavouriteType.Characters,
			response.User, user.Features);
		GetFavouritesEmbed(results, addedValues, removedValues, combinedResponse.Staff, Wrapper.Models.Enums.FavouriteType.Staff, response.User,
			user.Features);
		GetFavouritesEmbed(results, addedValues, removedValues, combinedResponse.Studios, Wrapper.Models.Enums.FavouriteType.Studios, response.User,
			user.Features);
		results.Sort((b1, b2) => b1.Color.Value.Value.CompareTo(b2.Color.Value.Value));
		return results;
	}
}