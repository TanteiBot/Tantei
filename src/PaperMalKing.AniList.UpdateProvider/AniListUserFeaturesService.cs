﻿// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaperMalKing.AniList.Wrapper.Abstractions;
using PaperMalKing.Database;
using PaperMalKing.Database.Models.AniList;
using PaperMalKing.UpdatesProviders.Base.Exceptions;
using PaperMalKing.UpdatesProviders.Base.Features;

namespace PaperMalKing.AniList.UpdateProvider;

internal sealed class AniListUserFeaturesService : BaseUserFeaturesService<AniListUser, AniListUserFeatures>
{
	private readonly IAniListClient _client;

	public AniListUserFeaturesService(IAniListClient client, ILogger<AniListUserFeaturesService> logger, IDbContextFactory<DatabaseContext> dbContextFactory)
		: base(dbContextFactory, logger)
	{
		this._client = client;
	}

	public override async Task EnableFeaturesAsync(AniListUserFeatures feature, ulong userId)
	{
		await using var db = this.DbContextFactory.CreateDbContext();
		var dbUser = db.AniListUsers.TagWith("Query user for enabling feature").TagWithCallSite().FirstOrDefault(u => u.DiscordUserId == userId) ??
					 throw new UserFeaturesException("You must register first before enabling features");
		if (dbUser.Features.HasFlag(feature))
		{
			throw new UserFeaturesException("You already have this feature enabled");
		}

		dbUser.Features |= feature;
		var now = TimeProvider.System.GetUtcNow().ToUnixTimeSeconds();
		switch (feature)
		{
			case AniListUserFeatures.AnimeList:
			case AniListUserFeatures.MangaList:
			{
				dbUser.LastActivityTimestamp = now;
				break;
			}

			case AniListUserFeatures.Favourites:
			{
				var fr = await this._client
								   .GetAllRecentUserUpdatesAsync(dbUser, AniListUserFeatures.Favourites | AniListUserFeatures.AnimeList, CancellationToken.None);
				dbUser.Favourites = fr.Favourites.ConvertAll(f => new AniListFavourite
				{
					Id = f.Id,
					FavouriteType = (FavouriteType)f.Type,
				});
				break;
			}

			case AniListUserFeatures.Reviews:
			{
				dbUser.LastReviewTimestamp = now;
				break;
			}
		}

		await db.SaveChangesAndThrowOnNoneAsync(CancellationToken.None);
	}

	protected override ValueTask DisableFeatureCleanupAsync(DatabaseContext db, AniListUser user, AniListUserFeatures featureToDisable)
	{
		if (featureToDisable == AniListUserFeatures.Favourites)
		{
			db.AniListFavourites.TagWith("Remove user's favorites when Favourites feature gets disabled").TagWithCallSite()
			  .Where(x => x.UserId == user.Id).ExecuteDelete();
		}

		return ValueTask.CompletedTask;
	}
}