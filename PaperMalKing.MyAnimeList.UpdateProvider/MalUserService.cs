// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaperMalKing.Database;
using PaperMalKing.Database.Models;
using PaperMalKing.Database.Models.MyAnimeList;
using PaperMalKing.MyAnimeList.Wrapper;
using PaperMalKing.UpdatesProviders.Base;
using PaperMalKing.UpdatesProviders.Base.Exceptions;

namespace PaperMalKing.UpdatesProviders.MyAnimeList;

public sealed class MalUserService : IUpdateProviderUserService
{
	private readonly MyAnimeListClient _client;

	private readonly ILogger<MalUserService> _logger;
	private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;


	public MalUserService(MyAnimeListClient client, ILogger<MalUserService> logger, IDbContextFactory<DatabaseContext> dbContextFactory)
	{
		this._client = client;
		this._logger = logger;
		this._dbContextFactory = dbContextFactory;
	}

	public static string Name => Constants.Name;

	public async Task<BaseUser> AddUserAsync(ulong userId, ulong guildId, string? username = null)
	{
		using var db = this._dbContextFactory.CreateDbContext();
		var dbUser = db.MalUsers.Include(u => u.DiscordUser).ThenInclude(du => du.Guilds)
					   .FirstOrDefault(u => u.DiscordUser.DiscordUserId == userId);
		DiscordGuild? guild;
		if (dbUser != null) // User already in DB
		{
			if (dbUser.DiscordUser.Guilds.Any(g => g.DiscordGuildId == guildId)) // User already in specified guild
			{
				throw new UserProcessingException(
					"You already have your account connected. If you want to switch to another account, remove current one, then add the new one.");
			}

			guild = db.DiscordGuilds.FirstOrDefault(g => g.DiscordGuildId == guildId);
			if (guild is null)
				throw new UserProcessingException(new(username ?? ""),
					"Current server is not in database, ask server administrator to add this server to bot");
			dbUser.DiscordUser.Guilds.Add(guild);
			db.MalUsers.Update(dbUser);
			await db.SaveChangesAndThrowOnNoneAsync().ConfigureAwait(false);
			return new(dbUser.Username);
		}

		guild = db.DiscordGuilds.FirstOrDefault(g => g.DiscordGuildId == guildId);
		if (guild is null)
			throw new UserProcessingException(new(username?? ""),
				"Current server is not in database, ask server administrator to add this server to bot");
		if (string.IsNullOrWhiteSpace(username))
		{
			throw new UserProcessingException(new(""), "You must provide username if you arent already tracked by this bot");
		}
		var duser = db.DiscordUsers.Include(x => x.Guilds).FirstOrDefault(user => user.DiscordUserId == userId);
		var mUser = await this._client.GetUserAsync(username, MalUserFeatures.None.GetDefault().ToParserOptions()).ConfigureAwait(false);
		var now = DateTimeOffset.Now;
		if (duser is null)
		{
			duser = new()
			{
				Guilds = new DiscordGuild[1] { guild },
				DiscordUserId = userId,
				BotUser = new()
			};
		}
		else if (duser.Guilds.All(x => x.DiscordGuildId != guildId))
		{
			duser.Guilds.Add(guild);
		}
		dbUser = new()
		{
			UserId = mUser.Id,
			Username = mUser.Username,
			DiscordUser = duser,
			LastUpdatedAnimeListTimestamp = now,
			LastUpdatedMangaListTimestamp = now,
			LastAnimeUpdateHash = mUser.LatestAnimeUpdateHash ?? "",
			LastMangaUpdateHash = mUser.LatestMangaUpdateHash ?? ""
		};
		dbUser.FavoriteAnimes = mUser.Favorites.FavoriteAnime.Select(anime => anime.ToMalFavoriteAnime(dbUser)).ToList();
		dbUser.FavoriteMangas = mUser.Favorites.FavoriteManga.Select(manga => manga.ToMalFavoriteManga(dbUser)).ToList();
		dbUser.FavoriteCharacters = mUser.Favorites.FavoriteCharacters.Select(character => character.ToMalFavoriteCharacter(dbUser)).ToList();
		dbUser.FavoritePeople = mUser.Favorites.FavoritePeople.Select(person => person.ToMalFavoritePerson(dbUser)).ToList();
		dbUser.FavoriteCompanies = mUser.Favorites.FavoriteCompanies.Select(company => company.ToMalFavoriteCompany(dbUser)).ToList();
		db.MalUsers.Add(dbUser);
		await db.SaveChangesAndThrowOnNoneAsync().ConfigureAwait(false);
		return new(dbUser.Username);
	}

	public async Task<BaseUser> RemoveUserAsync(ulong userId)
	{
		using var db = this._dbContextFactory.CreateDbContext();
		var user = db.MalUsers.Include(malUser => malUser.DiscordUser).ThenInclude(du => du.Guilds).Select(u => new MalUser
		{
			DiscordUser = u.DiscordUser,
			Username = u.Username,
			UserId = u.UserId
		}).FirstOrDefault(u => u.DiscordUser.DiscordUserId == userId);
		if (user is null)
		{
			throw new UserProcessingException($"You weren't tracked by {Name} update checker");
		}

		db.MalUsers.Remove(user);
		await db.SaveChangesAndThrowOnNoneAsync().ConfigureAwait(false);
		return new(user.Username);
	}

	public async Task RemoveUserHereAsync(ulong userId, ulong guildId)
	{
		using var db = this._dbContextFactory.CreateDbContext();
		var user = db.DiscordUsers.Include(du => du.Guilds).FirstOrDefault(du => du.DiscordUserId == userId);
		if (user is null)
			throw new UserProcessingException("You weren't registered in bot");
		var guild = user.Guilds.FirstOrDefault(g => g.DiscordGuildId == guildId);
		if (guild is null)
			throw new UserProcessingException("You weren't registered in this server");

		user.Guilds.Remove(guild);
		db.DiscordUsers.Update(user);
		await db.SaveChangesAndThrowOnNoneAsync().ConfigureAwait(false);
	}

	public IReadOnlyList<BaseUser> ListUsers(ulong guildId)
	{
		using var db = this._dbContextFactory.CreateDbContext();
		return db.MalUsers.Include(malUser => malUser.DiscordUser).ThenInclude(du => du.Guilds).Select(u => new MalUser
				 {
					 DiscordUser = u.DiscordUser,
					 Username = u.Username,
					 LastUpdatedAnimeListTimestamp = u.LastUpdatedAnimeListTimestamp
				 }).Where(u => u.DiscordUser.Guilds.Any(g => g.DiscordGuildId == guildId)).OrderByDescending(u => u.LastUpdatedAnimeListTimestamp)
				 .Select(mu => new BaseUser(mu.Username, mu.DiscordUser)).AsNoTracking().ToArray();
	}
}