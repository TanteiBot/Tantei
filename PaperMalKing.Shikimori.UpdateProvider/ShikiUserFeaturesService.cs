// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaperMalKing.Database;
using PaperMalKing.Database.Models.Shikimori;
using PaperMalKing.Shikimori.Wrapper;
using PaperMalKing.Shikimori.Wrapper.Models;
using PaperMalKing.UpdatesProviders.Base.Exceptions;
using PaperMalKing.UpdatesProviders.Base.Features;

namespace PaperMalKing.Shikimori.UpdateProvider;

public sealed class ShikiUserFeaturesService : IUserFeaturesService<ShikiUserFeatures>
{
	private readonly ShikiClient _client;
	private readonly ILogger<ShikiUserFeaturesService> _logger;
	private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

	public ShikiUserFeaturesService(ShikiClient client, ILogger<ShikiUserFeaturesService> logger, IDbContextFactory<DatabaseContext> dbContextFactory)
	{
		this._client = client;
		this._logger = logger;
		this._dbContextFactory = dbContextFactory;
	}

	public async Task EnableFeaturesAsync(ShikiUserFeatures feature, ulong userId)
	{
		using var db = this._dbContextFactory.CreateDbContext();
		var dbUser = db.ShikiUsers.Include(su => su.Favourites)
					   .FirstOrDefault(su => su.DiscordUserId == userId);
		if (dbUser is null)
			throw new UserFeaturesException("You must register first before enabling features");
		var lastHistoryEntry = new ulong?();
		dbUser.Features |= feature;
			switch (feature)
			{
				case ShikiUserFeatures.AnimeList:
				case ShikiUserFeatures.MangaList:
					{
						if (lastHistoryEntry.HasValue)
							break;
						var (data, _) = await this._client.GetUserHistoryAsync(dbUser.Id, 1, 1, HistoryRequestOptions.Any, CancellationToken.None)
												  .ConfigureAwait(false);
						lastHistoryEntry = data.MaxBy(h => h.Id)!.Id;
						break;
					}
				case ShikiUserFeatures.Favourites:
					{
						var favourites = await this._client.GetUserFavouritesAsync(dbUser.Id, CancellationToken.None).ConfigureAwait(false);
						dbUser.Favourites = favourites.AllFavourites.Select(fe => new ShikiFavourite
						{
							Id = fe.Id,
							Name = fe.Name,
							FavType = fe.GenericType!,
							User = dbUser
						}).ToList();
						break;
					}
		}

		if (lastHistoryEntry.HasValue)
			dbUser.LastHistoryEntryId = lastHistoryEntry.Value;
		db.ShikiUsers.Update(dbUser);
		await db.SaveChangesAndThrowOnNoneAsync(CancellationToken.None).ConfigureAwait(false);
	}

	public async Task DisableFeaturesAsync(ShikiUserFeatures feature, ulong userId)
	{
		using var db = this._dbContextFactory.CreateDbContext();
		var dbUser = db.ShikiUsers.Include(su => su.Favourites).FirstOrDefault(su => su.DiscordUserId == userId);
		if (dbUser is null)
			throw new UserFeaturesException("You must register first before disabling features");


		dbUser.Features &= ~feature;
		if (feature == ShikiUserFeatures.Favourites)
		{
			dbUser.Favourites.Clear();
		}

		db.ShikiUsers.Update(dbUser);
		await db.SaveChangesAndThrowOnNoneAsync(CancellationToken.None).ConfigureAwait(false);
	}

	public ValueTask<string> EnabledFeaturesAsync(ulong userId)
	{
		using var db = this._dbContextFactory.CreateDbContext();
		var dbUser = db.ShikiUsers.AsNoTrackingWithIdentityResolution().FirstOrDefault(su => su.DiscordUserId == userId);
		if (dbUser is null)
			throw new UserFeaturesException("You must register first before checking for enabled features");

		return ValueTask.FromResult(dbUser.Features.Humanize());
	}
}