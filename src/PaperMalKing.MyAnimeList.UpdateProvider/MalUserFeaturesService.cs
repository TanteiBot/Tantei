﻿// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.EntityFrameworkCore;
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
	private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;


	public MalUserFeaturesService(MyAnimeListClient client, ILogger<MalUserFeaturesService> logger, IDbContextFactory<DatabaseContext> dbContextFactory)
	{
		this._client = client;
		this._logger = logger;
		this._dbContextFactory = dbContextFactory;
	}

	public async Task EnableFeaturesAsync(MalUserFeatures feature, ulong userId)
	{
		using var db = this._dbContextFactory.CreateDbContext();
		var dbUser = db.MalUsers.FirstOrDefault(u => u.DiscordUser.DiscordUserId == userId);
		if (dbUser is null)
			throw new UserFeaturesException("You must register first before enabling features");
		if ((dbUser.Features & feature) != 0)
		{
			throw new UriFormatException("You already have this feature enabled");
		}

		User? user = null;
		dbUser.Features |= feature;
		var now = DateTimeOffset.UtcNow;

		switch (feature)
		{
			case MalUserFeatures.AnimeList:
			{
				user = await this._client.GetUserAsync(dbUser.Username, dbUser.Features.ToParserOptions(), CancellationToken.None)
								 .ConfigureAwait(false);
				dbUser.LastAnimeUpdateHash = user.LatestAnimeUpdateHash ?? "";
				dbUser.LastUpdatedAnimeListTimestamp = now;
				break;
			}
			case MalUserFeatures.MangaList:
			{
				user = await this._client.GetUserAsync(dbUser.Username, dbUser.Features.ToParserOptions(), CancellationToken.None)
								 .ConfigureAwait(false);
				dbUser.LastMangaUpdateHash = user.LatestMangaUpdateHash ?? "";
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

		await db.SaveChangesAndThrowOnNoneAsync(CancellationToken.None).ConfigureAwait(false);
	}

	public async Task DisableFeaturesAsync(MalUserFeatures feature, ulong userId)
	{
		using var db = this._dbContextFactory.CreateDbContext();
		var dbUser = db.MalUsers.FirstOrDefault(u => u.DiscordUser.DiscordUserId == userId);
		if (dbUser is null)
			throw new UserFeaturesException("You must register first before disabling features");
		if ((dbUser.Features & feature) != 0)
		{
			throw new UserFeaturesException("This feature wasnt enabled for you,so you cant enable it");
		}



		dbUser.Features &= ~feature;
		if (feature == MalUserFeatures.Favorites)
		{
			db.BaseMalFavorites.Where(x => x.UserId == dbUser.UserId).ExecuteDelete();
		}

		await db.SaveChangesAndThrowOnNoneAsync(CancellationToken.None).ConfigureAwait(false);
	}

	public ValueTask<string> EnabledFeaturesAsync(ulong userId)
	{
		using var db = this._dbContextFactory.CreateDbContext();
		var features = db.MalUsers.AsNoTracking().Where(u => u.DiscordUser.DiscordUserId == userId).Select(x=>new MalUserFeatures?(x.Features)).FirstOrDefault();
		if (!features.HasValue)
			throw new UserFeaturesException("You must register first before checking for enabled features");

		return ValueTask.FromResult(features.Value.Humanize());
	}
}