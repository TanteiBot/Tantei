// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaperMalKing.Database;
using PaperMalKing.Database.Models.Shikimori;
using PaperMalKing.Shikimori.Wrapper.Abstractions;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models;
using PaperMalKing.UpdatesProviders.Base.Exceptions;
using PaperMalKing.UpdatesProviders.Base.Features;

namespace PaperMalKing.Shikimori.UpdateProvider;

internal sealed class ShikiUserFeaturesService : BaseUserFeaturesService<ShikiUser, ShikiUserFeatures>
{
	private readonly IShikiClient _client;

	public ShikiUserFeaturesService(IShikiClient client, ILogger<ShikiUserFeaturesService> logger, IDbContextFactory<DatabaseContext> dbContextFactory)
		: base(dbContextFactory, logger)
	{
		this._client = client;
	}

	public override async Task EnableFeaturesAsync(ShikiUserFeatures feature, ulong userId)
	{
		await using var db = this.DbContextFactory.CreateDbContext();
		var dbUser = db.ShikiUsers.TagWith("Query user for enabling feature").TagWithCallSite().FirstOrDefault(su => su.DiscordUserId == userId) ??
					 throw new UserFeaturesException("You must register first before enabling features");
		if (dbUser.Features.HasFlag(feature))
		{
			throw new UserFeaturesException("You already have this feature enabled");
		}

		var lastHistoryEntry = default(uint?);
		dbUser.Features |= feature;
		switch (feature)
		{
			case ShikiUserFeatures.AnimeList:
			case ShikiUserFeatures.MangaList:
			{
				var (data, _) = await this._client.GetUserHistoryAsync(dbUser.Id, 1, 1, HistoryRequestOptions.Any, CancellationToken.None)
										  ;
				lastHistoryEntry = data.MaxBy(h => h.Id)!.Id;
				break;
			}

			case ShikiUserFeatures.Favourites:
			{
				var favourites = await this._client.GetUserFavouritesAsync(dbUser.Id, CancellationToken.None);
				dbUser.Favourites = favourites.AllFavourites.Select(fe => new ShikiFavourite
				{
					Id = fe.Id,
					Name = fe.Name,
					FavType = fe.GenericType!,
					User = dbUser,
				}).ToList();
				break;
			}

			case ShikiUserFeatures.Achievements:
			{
				var achievements = await this._client.GetUserAchievementsAsync(dbUser.Id);
				dbUser.Achievements = achievements.Select(x => new ShikiDbAchievement
				{
					NekoId = x.Id,
					Level = x.Level,
				}).ToList();
				break;
			}
		}

		if (lastHistoryEntry.HasValue)
		{
			dbUser.LastHistoryEntryId = lastHistoryEntry.Value;
		}

		await db.SaveChangesAndThrowOnNoneAsync(CancellationToken.None);
	}

	protected override ValueTask DisableFeatureCleanupAsync(DatabaseContext db, ShikiUser user, ShikiUserFeatures featureToDisable)
	{
		if (featureToDisable == ShikiUserFeatures.Favourites)
		{
			db.ShikiFavourites.TagWith("Remove user's favorites when Favourites feature gets disabled").TagWithCallSite().Where(x => x.UserId == user.Id).ExecuteDelete();
		}

		if (featureToDisable == ShikiUserFeatures.Achievements)
		{
			user.Achievements = [];
		}

		return ValueTask.CompletedTask;
	}
}