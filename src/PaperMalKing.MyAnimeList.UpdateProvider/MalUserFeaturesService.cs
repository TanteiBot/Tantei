// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaperMalKing.Database;
using PaperMalKing.Database.Models.MyAnimeList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models;
using PaperMalKing.UpdatesProviders.Base.Exceptions;
using PaperMalKing.UpdatesProviders.Base.Features;

namespace PaperMalKing.MyAnimeList.UpdateProvider;

internal sealed class MalUserFeaturesService(IMyAnimeListClient _client, ILogger<MalUserFeaturesService> logger, IDbContextFactory<DatabaseContext> dbContextFactory)
	: BaseUserFeaturesService<MalUser, MalUserFeatures>(dbContextFactory, logger)
{
	[SuppressMessage("Roslynator", "RCS1261:Resource can be disposed asynchronously", Justification = "Sqlite does not support async")]
	public override async Task EnableFeaturesAsync(MalUserFeatures feature, ulong userId)
	{
		using var db = this.DbContextFactory.CreateDbContext();
		var dbUser =
			db.MalUsers.TagWith("Query user for enabling feature").TagWithCallSite().FirstOrDefault(u => u.DiscordUser.DiscordUserId == userId) ??
			throw new UserFeaturesException("You must register first before enabling features");
		if (dbUser.Features.HasFlag(feature))
		{
			throw new UserFeaturesException("You already have this feature enabled");
		}

		User? user;
		dbUser.Features |= feature;
		var now = TimeProvider.System.GetUtcNow();

		switch (feature)
		{
			case MalUserFeatures.AnimeList:
			{
				user = await _client.GetUserAsync(dbUser.Username, dbUser.Features.ToParserOptions(), CancellationToken.None);
				dbUser.LastAnimeUpdateHash = user.LatestAnimeUpdateHash ?? "";
				dbUser.LastUpdatedAnimeListTimestamp = now;
				break;
			}

			case MalUserFeatures.MangaList:
			{
				user = await _client.GetUserAsync(dbUser.Username, dbUser.Features.ToParserOptions(), CancellationToken.None);
				dbUser.LastMangaUpdateHash = user.LatestMangaUpdateHash ?? "";
				dbUser.LastUpdatedMangaListTimestamp = now;
				break;
			}

			case MalUserFeatures.Favorites:
			{
				user = await _client.GetUserAsync(dbUser.Username, dbUser.Features.ToParserOptions(), CancellationToken.None);
				dbUser.FavoriteAnimes = user.Favorites.FavoriteAnime.Select(x => x.ToMalFavoriteAnime(dbUser)).ToList();
				dbUser.FavoriteMangas = user.Favorites.FavoriteManga.Select(x => x.ToMalFavoriteManga(dbUser)).ToList();
				dbUser.FavoriteCharacters = user.Favorites.FavoriteCharacters.Select(x => x.ToMalFavoriteCharacter(dbUser)).ToList();
				dbUser.FavoritePeople = user.Favorites.FavoritePeople.Select(x => x.ToMalFavoritePerson(dbUser)).ToList();
				dbUser.FavoriteCompanies = user.Favorites.FavoriteCompanies.Select(x => x.ToMalFavoriteCompany(dbUser)).ToList();
				break;
			}

			default:
			{
				// Ignore all other
				break;
			}
		}

		await db.SaveChangesAndThrowOnNoneAsync(CancellationToken.None);
	}

	protected override ValueTask DisableFeatureCleanupAsync(DatabaseContext db, MalUser user, MalUserFeatures featureToDisable)
	{
		if (featureToDisable == MalUserFeatures.Favorites)
		{
			db.BaseMalFavorites.TagWith("Remove user's favorites when Favourites feature gets disabled").TagWithCallSite().Where(x => x.UserId == user.UserId).ExecuteDelete();
		}

		return ValueTask.CompletedTask;
	}
}