// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Diagnostics.CodeAnalysis;
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

internal sealed class AniListUserFeaturesService(IAniListClient _client, ILogger<AniListUserFeaturesService> logger, IDbContextFactory<DatabaseContext> dbContextFactory)
	: BaseUserFeaturesService<AniListUser, AniListUserFeatures>(dbContextFactory, logger)
{
	[SuppressMessage("Roslynator", "RCS1261:Resource can be disposed asynchronously", Justification = "Sqlite does not support async")]
	public override async Task EnableFeaturesAsync(AniListUserFeatures feature, ulong userId)
	{
		using var db = this.DbContextFactory.CreateDbContext();
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
				var fr = await _client.GetAllRecentUserUpdatesAsync(dbUser, AniListUserFeatures.Favourites | AniListUserFeatures.AnimeList, CancellationToken.None);
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

			default:
			{
				// We dont care about others
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