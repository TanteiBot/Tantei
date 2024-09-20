// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaperMalKing.Common;
using PaperMalKing.Database;
using PaperMalKing.Database.Models;
using PaperMalKing.Database.Models.MyAnimeList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions;
using PaperMalKing.UpdatesProviders.Base;
using PaperMalKing.UpdatesProviders.Base.Exceptions;

namespace PaperMalKing.MyAnimeList.UpdateProvider;

internal sealed class MalUserService(IMyAnimeListClient _client, ILogger<MalUserService> logger, IDbContextFactory<DatabaseContext> dbContextFactory, GeneralUserService userService)
	: BaseUpdateProviderUserService<MalUser>(logger, dbContextFactory, userService)
{
	public override string Name => Constants.Name;

	[SuppressMessage("Roslynator", "RCS1261:Resource can be disposed asynchronously", Justification = "Sqlite does not support async")]
	public override async Task<BaseUser> AddUserAsync(ulong userId, ulong guildId, string? username = null)
	{
		using var db = this.DbContextFactory.CreateDbContext();
		var dbUser = db.MalUsers.TagWith("Query user when trying to add one").TagWithCallSite().Include(u => u.DiscordUser).ThenInclude(du => du.Guilds)
					   .FirstOrDefault(u => u.DiscordUserId == userId);
		DiscordGuild? guild;
		if (dbUser != null)
		{
			// User already in DB
			if (dbUser.DiscordUser.Guilds.Any(g => g.DiscordGuildId == guildId))
			{
				// User already in specified guild
				throw new UserProcessingException(
					"You already have your account connected. If you want to switch to another account, remove current one, then add the new one.");
			}

			guild = db.DiscordGuilds.TagWith("Query guild to add existing user to it").TagWithCallSite().FirstOrDefault(g => g.DiscordGuildId == guildId);
			if (guild is null)
			{
				throw new UserProcessingException(
					BaseUser.FromUsername(username),
					"Current server is not in database, ask server administrator to add this server to bot");
			}

			dbUser.DiscordUser.Guilds.Add(guild);
			await db.SaveChangesAndThrowOnNoneAsync();
			return BaseUser.FromUsername(dbUser.Username);
		}

		guild = db.DiscordGuilds.TagWith("Query guild to add new user to it").TagWithCallSite().FirstOrDefault(g => g.DiscordGuildId == guildId);
		if (guild is null)
		{
			throw new UserProcessingException(
				BaseUser.FromUsername(username),
				"Current server is not in database, ask server administrator to add this server to bot");
		}

		if (string.IsNullOrWhiteSpace(username))
		{
			throw new UserProcessingException(BaseUser.Empty, "You must provide username if you arent already tracked by this bot");
		}

		var duser = db.DiscordUsers.TagWith("Query discord user to link Mal user to it").TagWithCallSite().Include(x => x.Guilds).FirstOrDefault(user => user.DiscordUserId == userId);
		var mUser = await _client.GetUserAsync(username, MalUserFeatures.None.GetDefault().ToParserOptions());
		var now = TimeProvider.System.GetUtcNow();
		if (duser is null)
		{
			duser = new()
			{
				Guilds = [guild],
				DiscordUserId = userId,
				BotUser = new(),
			};
		}
		else if (duser.Guilds.All(x => x.DiscordGuildId != guildId))
		{
			duser.Guilds.Add(guild);
		}
		else
		{
			// User is already in guild, this case is handled above
		}

		dbUser = new()
		{
			UserId = mUser.Id,
			Username = mUser.Username,
			DiscordUser = duser,
			LastUpdatedAnimeListTimestamp = now,
			LastUpdatedMangaListTimestamp = now,
			LastAnimeUpdateHash = mUser.LatestAnimeUpdateHash ?? "",
			LastMangaUpdateHash = mUser.LatestMangaUpdateHash ?? "",
			FavoritesIdHash = HashHelpers.FavoritesHash(mUser.Favorites.GetFavoriteIdTypesFromFavorites()),
			Colors = [],
		};
		dbUser.FavoriteAnimes = mUser.Favorites.FavoriteAnime.Select(anime => anime.ToMalFavoriteAnime(dbUser)).ToList();
		dbUser.FavoriteMangas = mUser.Favorites.FavoriteManga.Select(manga => manga.ToMalFavoriteManga(dbUser)).ToList();
		dbUser.FavoriteCharacters = mUser.Favorites.FavoriteCharacters.Select(character => character.ToMalFavoriteCharacter(dbUser)).ToList();
		dbUser.FavoritePeople = mUser.Favorites.FavoritePeople.Select(person => person.ToMalFavoritePerson(dbUser)).ToList();
		dbUser.FavoriteCompanies = mUser.Favorites.FavoriteCompanies.Select(company => company.ToMalFavoriteCompany(dbUser)).ToList();
		db.MalUsers.Add(dbUser);
		await db.SaveChangesAndThrowOnNoneAsync();
		return BaseUser.FromUsername(dbUser.Username);
	}

	public override IReadOnlyList<BaseUser> ListUsers(ulong guildId)
	{
		return this.ListUsersCore(guildId, static u => u.LastUpdatedAnimeListTimestamp, static mu => new(mu.Username, mu.DiscordUser));
	}
}