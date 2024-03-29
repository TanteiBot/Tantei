﻿// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

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
using PaperMalKing.AniList.Wrapper.Abstractions;
using PaperMalKing.AniList.Wrapper.Abstractions.Models;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Interfaces;
using PaperMalKing.Common;
using PaperMalKing.Database;
using PaperMalKing.Database.Models.AniList;
using PaperMalKing.UpdatesProviders.Base;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;
using FavouriteType = PaperMalKing.Database.Models.AniList.FavouriteType;

namespace PaperMalKing.AniList.UpdateProvider;

internal sealed class AniListUpdateProvider : BaseUpdateProvider
{
	private readonly IAniListClient _client;
	private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

	public AniListUpdateProvider(ILogger<AniListUpdateProvider> logger, IOptions<AniListOptions> options, IAniListClient client, IDbContextFactory<DatabaseContext> dbContextFactory)
		: base(logger, TimeSpan.FromMilliseconds(options.Value.DelayBetweenChecksInMilliseconds))
	{
		this._client = client;
		this._dbContextFactory = dbContextFactory;
	}

	public override string Name => ProviderConstants.Name;

	public override event UpdateFoundEvent? UpdateFoundEvent;

	protected override async Task CheckForUpdatesAsync(CancellationToken cancellationToken)
	{
		if (this.UpdateFoundEvent is null)
		{
			return;
		}

		await using var db = this._dbContextFactory.CreateDbContext();
		var users = db.AniListUsers.TagWith("Query users for update checking").TagWithCallSite().Where(u =>
							u.DiscordUser.Guilds.Any() &&
							((u.Features & AniListUserFeatures.AnimeList) != 0 ||
							(u.Features & AniListUserFeatures.MangaList) != 0 ||
							(u.Features & AniListUserFeatures.Favourites) != 0 ||
							(u.Features & AniListUserFeatures.Reviews) != 0))
					  .OrderBy(static _ => EF.Functions.Random()).ToArray();
		foreach (var dbUser in users)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				break;
			}

			using var perUserCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
			perUserCancellationTokenSource.CancelAfter(TimeSpan.FromMinutes(3));
			var perUserCancellationToken = perUserCancellationTokenSource.Token;
			this.Logger.StartingToCheckUpdatesFor(dbUser.Id);
			var allUpdates = new List<DiscordEmbedBuilder>();
			CombinedRecentUpdatesResponse recentUserUpdates;
			try
			{
				recentUserUpdates = await this._client.GetAllRecentUserUpdatesAsync(dbUser, dbUser.Features, perUserCancellationToken)
											  ;
			}
			catch (GraphQLHttpRequestException ex) when (ex.Message.Contains("NotFound", StringComparison.Ordinal))
			{
				continue;
			}

			var isFavouritesHashMismatch = !string.Equals(
				dbUser.FavouritesIdHash,
				HashHelpers.FavoritesHash(recentUserUpdates.Favourites
														   .Select(x => new FavoriteIdType(x.Id, (byte)x.Type))
														   .OrderBy(x => x.Id).ThenBy(x => x.Type).ToArray()),
				StringComparison.Ordinal);

			if (dbUser.Features.HasFlag(AniListUserFeatures.Favourites) && isFavouritesHashMismatch)
			{
				allUpdates.AddRange(await this.GetFavouritesUpdatesAsync(recentUserUpdates, dbUser, db, perUserCancellationToken));
			}

			if (dbUser.Features.HasFlag(AniListUserFeatures.Reviews))
			{
				allUpdates.AddRange(recentUserUpdates.Reviews.Where(r => r.CreatedAtTimeStamp > dbUser.LastReviewTimestamp)
													 .Select(r => r.ToDiscordEmbedBuilder(recentUserUpdates.User, dbUser)));
			}

			foreach (var grouping in recentUserUpdates.Activities.GroupBy(activity => activity.Media.Id))
			{
				var lastListActivityOnMedia = grouping.MaxBy(activity => activity.CreatedAtTimestamp)!;
				var mediaListEntry = lastListActivityOnMedia.Media.Type == ListType.ANIME
					? recentUserUpdates.AnimeList.Find(mle => mle.Id == lastListActivityOnMedia.Media.Id)
					: recentUserUpdates.MangaList.Find(mle => mle.Id == lastListActivityOnMedia.Media.Id);
				if (mediaListEntry is not null)
				{
					allUpdates.Add(lastListActivityOnMedia.ToDiscordEmbedBuilder(mediaListEntry, recentUserUpdates.User, dbUser));
				}
			}

			if (allUpdates is [])
			{
				this.Logger.NoUpdatesFound(recentUserUpdates.User.Name);
				db.Entry(dbUser).State = EntityState.Unchanged;
				continue;
			}

			db.Entry(dbUser).Reference(u => u.DiscordUser).Load();
			db.Entry(dbUser.DiscordUser).Collection(du => du.Guilds).Load();
			var lastActivityTimestamp = recentUserUpdates.Activities is not [] ? recentUserUpdates.Activities.Max(a => a.CreatedAtTimestamp) : 0L;
			var lastReviewTimeStamp = recentUserUpdates.Reviews is not [] ? recentUserUpdates.Reviews.Max(r => r.CreatedAtTimeStamp) : 0L;
			if (dbUser.LastActivityTimestamp < lastActivityTimestamp)
			{
				dbUser.LastActivityTimestamp = lastActivityTimestamp;
			}

			if (dbUser.LastReviewTimestamp < lastReviewTimeStamp)
			{
				dbUser.LastReviewTimestamp = lastReviewTimeStamp;
			}

			if (dbUser.Features.HasFlag(AniListUserFeatures.Mention))
			{
				allUpdates.ForEach(u => u.AddField("By", DiscordHelpers.ToDiscordMention(dbUser.DiscordUserId), inline: true));
			}

			if (dbUser.Features.HasFlag(AniListUserFeatures.Website))
			{
				allUpdates.ForEach(u => u.WithAniListFooter());
			}

			allUpdates.SortBy(u => u.Timestamp.GetValueOrDefault());
			if (perUserCancellationToken.IsCancellationRequested)
			{
				db.Entry(dbUser).State = EntityState.Unchanged;
				this.Logger.CancellationRequested();
				break;
			}

			try
			{
				if (db.SaveChanges() <= 0)
				{
					throw new NoChangesSavedException();
				}

				await this.UpdateFoundEvent.Invoke(new(new BaseUpdate(allUpdates), this, dbUser.DiscordUser));
				if (isFavouritesHashMismatch)
				{
					dbUser.FavouritesIdHash = HashHelpers.FavoritesHash(db.AniListFavourites.TagWith("Query users favorites hash for updating")
																		  .TagWithCallSite().Where(x => x.UserId == dbUser.Id).OrderBy(x => x.Id)
																		  .ThenBy(x => x.FavouriteType)
																		  .Select(x => new FavoriteIdType(x.Id, (byte)x.FavouriteType)).ToArray());
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

	private async Task<IReadOnlyList<DiscordEmbedBuilder>> GetFavouritesUpdatesAsync(CombinedRecentUpdatesResponse response, AniListUser user, DatabaseContext db, CancellationToken cancellationToken)
	{
		Func<IdentifiableFavourite, AniListFavourite> FavouritesSelector(AniListUser aniListUser)
		{
			return av => new()
			{
				User = aniListUser,
				UserId = aniListUser.Id,
				Id = av.Id,
				FavouriteType = (FavouriteType)av.Type,
			};
		}

		static uint[] GetIds(IReadOnlyList<IdentifiableFavourite> collection, Func<IdentifiableFavourite, bool> predicate)
		{
			return collection.Any(predicate) ? collection.Where(predicate).Select(f => f.Id).ToArray() : [];
		}

		static void GetFavouritesEmbed<T>(
										List<DiscordEmbedBuilder> aggregator,
										IReadOnlyList<IdentifiableFavourite> addedValues,
										IReadOnlyList<IdentifiableFavourite> removedValues,
										List<T> obtainedValues,
										Wrapper.Abstractions.Models.Enums.FavouriteType type,
										User user,
										AniListUser dbUser)
			where T : class, IIdentifiable, ISiteUrlable
		{
			foreach (var value in obtainedValues)
			{
				bool? added = null;
				if (addedValues.Any(f => f.Id == value.Id && f.Type == type))
				{
					added = true;
				}
				else if (removedValues.Any(f => f.Id == value.Id && f.Type == type))
				{
					added = false;
				}

				if (added.HasValue)
				{
					aggregator.Add(FavouriteToDiscordEmbedBuilderConverter.Convert(value, user, added.Value, dbUser));
				}
			}
		}

		var convFavs = db.AniListFavourites.TagWith("Query current user favorites").TagWithCallSite().Where(x => x.UserId == user.Id).Select(f => new IdentifiableFavourite
		{
			Id = f.Id,
			Type = (Wrapper.Abstractions.Models.Enums.FavouriteType)f.FavouriteType,
		}).ToArray();

		var (addedValues, removedValues) = convFavs.GetDifference(response.Favourites);
		if (cancellationToken.IsCancellationRequested || (addedValues is [] && removedValues is []))
		{
			return [];
		}

		db.AniListFavourites.RemoveRange(removedValues.Select(FavouritesSelector(user)));
		db.AniListFavourites.AddRange(addedValues.Select(FavouritesSelector(user)));

		var changedValues = new List<IdentifiableFavourite>(addedValues.Count + removedValues.Count);
		changedValues.AddRange(addedValues);
		changedValues.AddRange(removedValues);
		var animeIds = GetIds(changedValues, f => f.Type == Wrapper.Abstractions.Models.Enums.FavouriteType.Anime);
		var mangaIds = GetIds(changedValues, f => f.Type == Wrapper.Abstractions.Models.Enums.FavouriteType.Manga);
		var charIds = GetIds(changedValues, f => f.Type == Wrapper.Abstractions.Models.Enums.FavouriteType.Characters);
		var staffIds = GetIds(changedValues, f => f.Type == Wrapper.Abstractions.Models.Enums.FavouriteType.Staff);
		var studioIds = GetIds(changedValues, f => f.Type == Wrapper.Abstractions.Models.Enums.FavouriteType.Studios);
		var hasNextPage = true;
		var combinedResponse = new CombinedFavouritesInfoResponse();
		var results = new List<DiscordEmbedBuilder>(changedValues.Count);
		for (byte page = 1; hasNextPage; page++)
		{
			var favouritesInfo = await this._client
										   .FavouritesInfoAsync(page, animeIds, mangaIds, charIds, staffIds, studioIds, (RequestOptions)user.Features, cancellationToken);
			combinedResponse.Add(favouritesInfo);
			hasNextPage = favouritesInfo.HasNextPage;
		}

		GetFavouritesEmbed(results, addedValues, removedValues, combinedResponse.Anime, Wrapper.Abstractions.Models.Enums.FavouriteType.Anime, response.User, user);
		GetFavouritesEmbed(results, addedValues, removedValues, combinedResponse.Manga, Wrapper.Abstractions.Models.Enums.FavouriteType.Manga, response.User, user);
		GetFavouritesEmbed(results, addedValues, removedValues, combinedResponse.Characters, Wrapper.Abstractions.Models.Enums.FavouriteType.Characters, response.User, user);
		GetFavouritesEmbed(results, addedValues, removedValues, combinedResponse.Staff, Wrapper.Abstractions.Models.Enums.FavouriteType.Staff, response.User, user);
		GetFavouritesEmbed(results, addedValues, removedValues, combinedResponse.Studios, Wrapper.Abstractions.Models.Enums.FavouriteType.Studios, response.User, user);
		results.SortBy(x => x.Color.Value.Value);
		return results;
	}
}