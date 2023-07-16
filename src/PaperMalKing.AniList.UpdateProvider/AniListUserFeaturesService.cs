// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

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

	public AniListUserFeaturesService(IAniListClient client, ILogger<AniListUserFeaturesService> logger,
									  IDbContextFactory<DatabaseContext> dbContextFactory) : base(dbContextFactory, logger)
	{
		this._client = client;
	}

	public override async Task EnableFeaturesAsync(AniListUserFeatures feature, ulong userId)
	{
		using var db = this.DbContextFactory.CreateDbContext();
		var dbUser = db.AniListUsers.TagWith("Query user for enabling feature").TagWithCallSite().FirstOrDefault(u => u.DiscordUserId == userId);
		if (dbUser is null)
			throw new UserFeaturesException("You must register first before enabling features");
		if (dbUser.Features.HasFlag(feature))
		{
			throw new UriFormatException("You already have this feature enabled");
		}

		dbUser.Features |= feature;
		var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
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
								   .GetAllRecentUserUpdatesAsync(dbUser, AniListUserFeatures.Favourites | AniListUserFeatures.AnimeList,
									   CancellationToken.None).ConfigureAwait(false);
				dbUser.Favourites = fr.Favourites.ConvertAll(f => new AniListFavourite
				{
					Id = f.Id,
					FavouriteType = (FavouriteType)f.Type
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
				// None additional actions needed
				break;
			}
		}

		await db.SaveChangesAndThrowOnNoneAsync(CancellationToken.None).ConfigureAwait(false);
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