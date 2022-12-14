// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PaperMalKing.AniList.Wrapper;
using PaperMalKing.Database;
using PaperMalKing.Database.Models.AniList;
using PaperMalKing.UpdatesProviders.Base.Exceptions;
using PaperMalKing.UpdatesProviders.Base.Features;

namespace PaperMalKing.AniList.UpdateProvider;

public sealed class AniListUserFeaturesService : IUserFeaturesService<AniListUserFeatures>
{
	private readonly AniListClient _client;
	private readonly ILogger<AniListUserFeaturesService> _logger;
	private readonly IServiceProvider _serviceProvider;

	public AniListUserFeaturesService(AniListClient client, ILogger<AniListUserFeaturesService> logger, IServiceProvider serviceProvider)
	{
		this._client = client;
		this._logger = logger;
		this._serviceProvider = serviceProvider;
	}

	public async Task EnableFeaturesAsync(AniListUserFeatures feature, ulong userId)
	{
		using var scope = this._serviceProvider.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
		var dbUser = db.AniListUsers.Include(u => u.Favourites).FirstOrDefault(u => u.DiscordUserId == userId);
		if (dbUser is null)
			throw new UserFeaturesException("You must register first before enabling features");
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
				var fr = await this._client.GetAllRecentUserUpdatesAsync(dbUser, AniListUserFeatures.Favourites, CancellationToken.None)
								   .ConfigureAwait(false);
				dbUser.Favourites.Clear();
				dbUser.Favourites.AddRange(fr.Favourites.Select(f => new AniListFavourite
				{
					Id = f.Id,
					FavouriteType = (FavouriteType)f.Type
				}).ToList());
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


		db.AniListUsers.Update(dbUser);
		await db.SaveChangesAndThrowOnNoneAsync(CancellationToken.None).ConfigureAwait(false);
	}

	public async Task DisableFeaturesAsync(AniListUserFeatures feature, ulong userId)
	{
		using var scope = this._serviceProvider.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
		var dbUser = db.AniListUsers.Include(su => su.Favourites).FirstOrDefault(su => su.DiscordUserId == userId);
		if (dbUser is null)
			throw new UserFeaturesException("You must register first before disabling features");

		dbUser.Features &= ~feature;
		if (feature == AniListUserFeatures.Favourites)
			dbUser.Favourites.Clear();

		db.AniListUsers.Update(dbUser);
		await db.SaveChangesAndThrowOnNoneAsync(CancellationToken.None).ConfigureAwait(false);
	}

	public ValueTask<string> EnabledFeaturesAsync(ulong userId)
	{
		using var scope = this._serviceProvider.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
		var dbUser = db.AniListUsers.AsNoTrackingWithIdentityResolution().FirstOrDefault(su => su.DiscordUserId == userId);
		if (dbUser is null)
			throw new UserFeaturesException("You must register first before checking for enabled features");

		return ValueTask.FromResult(dbUser.Features.Humanize());
	}
}