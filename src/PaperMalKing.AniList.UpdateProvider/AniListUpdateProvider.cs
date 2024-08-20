// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using GraphQL.Client.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Threading;
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

internal sealed class AniListUpdateProvider(ILogger<AniListUpdateProvider> logger, IOptions<AniListOptions> options, IAniListClient _client, IDbContextFactory<DatabaseContext> _dbContextFactory)
	: BaseUpdateProvider(logger, TimeSpan.FromMilliseconds(options.Value.DelayBetweenChecksInMilliseconds))
{
	public override string Name => ProviderConstants.Name;

	public override event AsyncEventHandler<UpdateFoundEventArgs>? UpdateFoundEvent;

	protected override async Task CheckForUpdatesAsync(CancellationToken cancellationToken)
	{
		if (this.UpdateFoundEvent is null)
		{
			return;
		}

		await using var db = _dbContextFactory.CreateDbContext();
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
			CombinedRecentUpdatesResponse recentUserUpdates;
			try
			{
				recentUserUpdates = await _client.GetAllRecentUserUpdatesAsync(dbUser, dbUser.Features, perUserCancellationToken);
			}
			catch (GraphQLHttpRequestException ex) when (ex.Message.Contains("NotFound", StringComparison.Ordinal))
			{
				continue;
			}

			var isFavouritesHashMismatch = !dbUser.FavouritesIdHash.Equals(HashHelpers.FavoritesHash(recentUserUpdates.Favourites.ToFavoriteIdType()), StringComparison.Ordinal);

			var favorites = (isFavouritesHashMismatch && dbUser.Features.HasFlag(AniListUserFeatures.Favourites)) ? await this.GetFavouritesUpdatesAsync(recentUserUpdates, dbUser, db, cancellationToken) : [];

			if ((dbUser.Features.HasFlag(AniListUserFeatures.Favourites) && isFavouritesHashMismatch) ||
				(dbUser.Features.HasFlag(AniListUserFeatures.Reviews) && recentUserUpdates.Reviews.Exists(r => r.CreatedAtTimeStamp > dbUser.LastReviewTimestamp)) ||
				(dbUser.Features.HasFlag(AniListUserFeatures.AnimeList) && recentUserUpdates.Activities.Exists(a => a.Media.Type == ListType.ANIME && a.CreatedAtTimestamp > dbUser.LastActivityTimestamp)) ||
				(dbUser.Features.HasFlag(AniListUserFeatures.MangaList) && recentUserUpdates.Activities.Exists(a => a.Media.Type == ListType.MANGA && a.CreatedAtTimestamp > dbUser.LastActivityTimestamp)))
			{
				db.Entry(dbUser).Reference(u => u.DiscordUser).Load();
				db.Entry(dbUser.DiscordUser).Collection(du => du.Guilds).Load();
				await this.UpdateFoundEvent.InvokeAsync(this, new(new BaseUpdate(this.GetUpdatesAsync(recentUserUpdates, favorites, dbUser, db, perUserCancellationToken)), dbUser.DiscordUser));
			}
			else
			{
				this.Logger.NoUpdatesFound(recentUserUpdates.User.Name);
				db.Entry(dbUser).State = EntityState.Unchanged;
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

		static void GetFavouritesEmbed<T>(List<DiscordEmbedBuilder> aggregator,
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
			var favouritesInfo = await _client.FavouritesInfoAsync(page, animeIds, mangaIds, charIds, staffIds, studioIds, (RequestOptions)user.Features, cancellationToken);
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

	private async IAsyncEnumerable<DiscordEmbedBuilder> GetUpdatesAsync(CombinedRecentUpdatesResponse recentUserUpdates,
																		IReadOnlyList<DiscordEmbedBuilder> favorites,
																		AniListUser dbUser,
																		DatabaseContext db,
																		[EnumeratorCancellation] CancellationToken cancellationToken)
	{
		static DiscordEmbedBuilder FormatEmbed(DiscordEmbedBuilder builder, AniListUser user)
		{
			if (user.Features.HasFlag(AniListUserFeatures.Mention))
			{
				builder.AddField("By", DiscordHelpers.ToDiscordMention(user.DiscordUserId), inline: true);
			}

			if (user.Features.HasFlag(AniListUserFeatures.Website))
			{
				builder.WithAniListFooter();
			}

			return builder;
		}

		int updatesCount = 0;

		if (favorites.Any())
		{
			foreach (var deb in favorites)
			{
				yield return FormatEmbed(deb, dbUser);
				updatesCount++;
			}

			await db.SaveChangesAndThrowOnNoneAsync(cancellationToken);

			dbUser.FavouritesIdHash = HashHelpers.FavoritesHash(db.AniListFavourites.TagWith("Query users favorites hash for updating")
																  .TagWithCallSite().Where(x => x.UserId == dbUser.Id).OrderBy(x => x.Id)
																  .ThenBy(x => x.FavouriteType)
																  .Select(x => new FavoriteIdType(x.Id, (byte)x.FavouriteType)).ToArray());
			await db.SaveChangesAndThrowOnNoneAsync(cancellationToken);
		}

		if (cancellationToken.IsCancellationRequested)
		{
			db.Entry(dbUser).State = EntityState.Unchanged;
			this.Logger.CancellationRequested();
			yield break;
		}

		if (dbUser.Features.HasFlag(AniListUserFeatures.Reviews))
		{
			recentUserUpdates.Reviews.RemoveAll(r => r.CreatedAtTimeStamp <= dbUser.LastReviewTimestamp);

			if (recentUserUpdates.Reviews is not [])
			{
				foreach (var deb in recentUserUpdates.Reviews)
				{
					yield return FormatEmbed(deb.ToDiscordEmbedBuilder(recentUserUpdates.User, dbUser), dbUser);
					updatesCount++;
				}

				dbUser.LastReviewTimestamp = recentUserUpdates.Reviews.Max(r => r.CreatedAtTimeStamp);
				await db.SaveChangesAndThrowOnNoneAsync(cancellationToken);
			}
		}

		if (cancellationToken.IsCancellationRequested)
		{
			db.Entry(dbUser).State = EntityState.Unchanged;
			this.Logger.CancellationRequested();
			yield break;
		}

		if (dbUser.Features.HasFlag(AniListUserFeatures.AnimeList) || dbUser.Features.HasFlag(AniListUserFeatures.MangaList))
		{
			foreach (var grouping in recentUserUpdates.Activities.GroupBy(activity => activity.Media.Id).OrderBy(a => a.Max(aa => aa.CreatedAtTimestamp)))
			{
				var lastListActivityOnMedia = grouping.MaxBy(activity => activity.CreatedAtTimestamp)!;
				var mediaListEntry = lastListActivityOnMedia.Media.Type == ListType.ANIME
					? recentUserUpdates.AnimeList.Find(mle => mle.Id == lastListActivityOnMedia.Media.Id)
					: recentUserUpdates.MangaList.Find(mle => mle.Id == lastListActivityOnMedia.Media.Id);
				if (mediaListEntry is not null)
				{
					var embed = lastListActivityOnMedia.ToDiscordEmbedBuilder(mediaListEntry, recentUserUpdates.User, dbUser);

					yield return FormatEmbed(embed, dbUser);

					updatesCount++;

					dbUser.LastActivityTimestamp = lastListActivityOnMedia.CreatedAtTimestamp;
					await db.SaveChangesAndThrowOnNoneAsync(cancellationToken);

					if (cancellationToken.IsCancellationRequested)
					{
						db.Entry(dbUser).State = EntityState.Unchanged;
						this.Logger.CancellationRequested();
						yield break;
					}
				}
			}
		}

		this.Logger.FoundUpdatesForUser(updatesCount, recentUserUpdates.User.Name ?? dbUser.Id.ToString(CultureInfo.InvariantCulture));
	}
}