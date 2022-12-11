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
using PaperMalKing.Database;
using PaperMalKing.Database.Models.MyAnimeList;
using PaperMalKing.MyAnimeList.Wrapper;
using PaperMalKing.MyAnimeList.Wrapper.Models;
using PaperMalKing.UpdatesProviders.Base.Exceptions;
using PaperMalKing.UpdatesProviders.Base.Features;

namespace PaperMalKing.UpdatesProviders.MyAnimeList;

internal sealed class MalUserFeaturesService : IUserFeaturesService<MalUserFeatures>
{
	private readonly MyAnimeListClient _client;
	private readonly ILogger<MalUserFeaturesService> _logger;
	private readonly IServiceProvider _serviceProvider;


	public MalUserFeaturesService(MyAnimeListClient client, ILogger<MalUserFeaturesService> logger, IServiceProvider serviceProvider)
	{
		this._client = client;
		this._logger = logger;
		this._serviceProvider = serviceProvider;
	}

	public async Task EnableFeaturesAsync(MalUserFeatures feature, ulong userId)
	{
		using var scope = this._serviceProvider.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
		var dbUser = db.MalUsers.Include(mu => mu.DiscordUser).Include(u => u.FavoriteAnimes).Include(u => u.FavoriteMangas)
					   .Include(u => u.FavoriteCharacters).Include(u => u.FavoritePeople).Include(u => u.FavoriteCompanies)
					   .FirstOrDefault(u => u.DiscordUser.DiscordUserId == userId);
		if (dbUser is null)
			throw new UserFeaturesException("You must register first before enabling features");
		User? user = null;
		dbUser.Features |= feature;
		var now = DateTimeOffset.UtcNow;

		switch (feature)
		{
			case MalUserFeatures.AnimeList:
			{
				user = await this._client.GetUserAsync(dbUser.Username, dbUser.Features.ToParserOptions(), CancellationToken.None)
								 .ConfigureAwait(false);
				dbUser.LastAnimeUpdateHash = user.LatestAnimeUpdate?.Hash.ToHashString() ?? "";
				dbUser.LastUpdatedAnimeListTimestamp = now;
				break;
			}
			case MalUserFeatures.MangaList:
			{
				user = await this._client.GetUserAsync(dbUser.Username, dbUser.Features.ToParserOptions(), CancellationToken.None)
								 .ConfigureAwait(false);
				dbUser.LastAnimeUpdateHash = user.LatestAnimeUpdate?.Hash.ToHashString() ?? "";
				dbUser.LastUpdatedMangaListTimestamp = now;
				break;
			}
			case MalUserFeatures.Favorites:
			{
				user = await this._client.GetUserAsync(dbUser.Username, dbUser.Features.ToParserOptions(), CancellationToken.None)
								 .ConfigureAwait(false);
				dbUser.FavoriteAnimes = user.Favorites.FavoriteAnime.Select(x => x.ToMalFavoriteAnime(dbUser)).ToList();
				dbUser.FavoriteMangas = user.Favorites.FavoriteManga.Select(x => x.ToMalFavoriteManga(dbUser)).ToList();
				dbUser.FavoriteCharacters = user.Favorites.FavoriteCharacters.Select(x => x.ToMalFavoriteCharacter(dbUser)).ToList();
				dbUser.FavoritePeople = user.Favorites.FavoritePeople.Select(x => x.ToMalFavoritePerson(dbUser)).ToList();
				dbUser.FavoriteCompanies = user.Favorites.FavoriteCompanies.Select(x => x.ToMalFavoriteCompany(dbUser)).ToList();
				break;
			}
			default:
			{
				// No additional work needed
				break;
			}
		}


		db.MalUsers.Update(dbUser);
		await db.SaveChangesAndThrowOnNoneAsync(CancellationToken.None).ConfigureAwait(false);
	}

	public async Task DisableFeaturesAsync(MalUserFeatures feature, ulong userId)
	{
		using var scope = this._serviceProvider.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
		var dbUser = db.MalUsers.Include(mu => mu.DiscordUser).Include(u => u.FavoriteAnimes).Include(u => u.FavoriteMangas)
					   .Include(u => u.FavoriteCharacters).Include(u => u.FavoritePeople).Include(u => u.FavoriteCompanies)
					   .FirstOrDefault(u => u.DiscordUser.DiscordUserId == userId);
		if (dbUser is null)
			throw new UserFeaturesException("You must register first before disabling features");


		dbUser.Features &= ~feature;
		if (feature == MalUserFeatures.Favorites)
		{
			dbUser.FavoriteAnimes.Clear();
			dbUser.FavoriteMangas.Clear();
			dbUser.FavoriteCharacters.Clear();
			dbUser.FavoritePeople.Clear();
			dbUser.FavoriteCompanies.Clear();
		}

		db.MalUsers.Update(dbUser);
		await db.SaveChangesAndThrowOnNoneAsync(CancellationToken.None).ConfigureAwait(false);
	}

	public ValueTask<string> EnabledFeaturesAsync(ulong userId)
	{
		using var scope = this._serviceProvider.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
		var dbUser = db.MalUsers.Include(mu => mu.DiscordUser).AsNoTrackingWithIdentityResolution()
					   .FirstOrDefault(u => u.DiscordUser.DiscordUserId == userId);
		if (dbUser is null)
			throw new UserFeaturesException("You must register first before checking for enabled features");

		return ValueTask.FromResult(dbUser.Features.Humanize());
	}
}