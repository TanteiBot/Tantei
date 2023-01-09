// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaperMalKing.Common;
using PaperMalKing.Database;
using PaperMalKing.Database.Models;
using PaperMalKing.Database.Models.Shikimori;
using PaperMalKing.Shikimori.Wrapper;
using PaperMalKing.Shikimori.Wrapper.Models;
using PaperMalKing.UpdatesProviders.Base;
using PaperMalKing.UpdatesProviders.Base.Exceptions;

namespace PaperMalKing.Shikimori.UpdateProvider;

internal sealed class ShikiUserService : IUpdateProviderUserService
{
	private readonly ShikiClient _client;
	private readonly ILogger<ShikiUserService> _logger;
	private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

	public static string Name => Constants.NAME;

	public ShikiUserService(ShikiClient client, ILogger<ShikiUserService> logger, IDbContextFactory<DatabaseContext> dbContextFactory)
	{
		this._client = client;
		this._logger = logger;
		this._dbContextFactory = dbContextFactory;
	}

	public async Task<BaseUser> AddUserAsync(ulong userId, ulong guildId, string? username = null)
	{
		using var db = this._dbContextFactory.CreateDbContext();
		var dbUser = db.ShikiUsers.Include(su => su.DiscordUser).ThenInclude(du => du.Guilds).FirstOrDefault(su => su.DiscordUserId == userId);
		DiscordGuild? guild;
		if (dbUser != null) // User already in db
		{
			if (dbUser.DiscordUser.Guilds.Any(g => g.DiscordGuildId == guildId))
				throw new UserProcessingException(
					"You already have your account connected. If you want to switch to another account, remove current one, then add the new one.");
			guild = db.DiscordGuilds.FirstOrDefault(g => g.DiscordGuildId == guildId);
			if (guild is null)
				throw new UserProcessingException(BaseUser.FromUsername(username), 
					"Current server is not in database, ask server administrator to add this server to bot");

			dbUser.DiscordUser.Guilds.Add(guild);
			await db.SaveChangesAndThrowOnNoneAsync().ConfigureAwait(false);
			return BaseUser.FromUsername(username);
		}

		guild = db.DiscordGuilds.FirstOrDefault(g => g.DiscordGuildId == guildId);
		if (guild is null)
			throw new UserProcessingException(BaseUser.FromUsername(username),
				"Current server is not in database, ask server administrator to add this server to bot");
		if (string.IsNullOrWhiteSpace(username))
		{
			throw new UserProcessingException(BaseUser.Empty, "You must provide username if you arent already tracked by this bot");
		}
		var dUser = db.DiscordUsers.Include(x => x.Guilds).FirstOrDefault(du => du.DiscordUserId == userId);
		var shikiUser = await this._client.GetUserAsync(username).ConfigureAwait(false);
		var history = await this._client.GetUserHistoryAsync(shikiUser.Id, 1, 1, HistoryRequestOptions.Any).ConfigureAwait(false);
		var favourites = await this._client.GetUserFavouritesAsync(shikiUser.Id).ConfigureAwait(false);
		if (dUser is null)
		{
			dUser = new()
			{
				BotUser = new(),
				Guilds = new DiscordGuild[1] { guild },
				DiscordUserId = userId,
			};
		}
		else if (dUser.Guilds.All(x => x.DiscordGuildId != guildId))
		{
			dUser.Guilds.Add(guild);
		}
		dbUser = new()
		{
			Favourites = favourites.AllFavourites.Select(f => new ShikiFavourite()
			{
				Id = f.Id,
				Name = f.Name,
				FavType = f.GenericType!
				// User = dbUser
			}).ToList(),
			Id = shikiUser.Id,
			Features = ShikiUserFeatures.None.GetDefault(),
			DiscordUser = dUser,
			DiscordUserId = userId,
			LastHistoryEntryId = history.Data.Max(he => he.Id),
			FavouritesIdHash = Helpers.FavoritesHash(favourites.AllFavourites.Select(x=> new FavoriteIdType(x.Id, (byte)x.GenericType![0])).ToArray())
		};
		dbUser.Favourites.ForEach(f => f.User = dbUser);
		db.ShikiUsers.Add(dbUser);
		await db.SaveChangesAndThrowOnNoneAsync().ConfigureAwait(false);
		return BaseUser.FromUsername(username);
	}

	public async Task<BaseUser> RemoveUserAsync(ulong userId)
	{
		using var db = this._dbContextFactory.CreateDbContext();
		var user = db.ShikiUsers.FirstOrDefault(su => su.DiscordUserId == userId);
		if (user is null)
			throw new UserProcessingException($"You weren't tracked by {Name} update checker");

		db.ShikiUsers.Remove(user);
		await db.SaveChangesAndThrowOnNoneAsync().ConfigureAwait(false);
		return BaseUser.Empty;
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
		await db.SaveChangesAndThrowOnNoneAsync().ConfigureAwait(false);
	}

	public IReadOnlyList<BaseUser> ListUsers(ulong guildId)
	{
		using var db = this._dbContextFactory.CreateDbContext();
		return db.ShikiUsers.Include(su => su.DiscordUser).OrderByDescending(u => u.LastHistoryEntryId)
				 .Where(u => u.DiscordUser.Guilds.Any(g => g.DiscordGuildId == guildId)).Select(u => new BaseUser("", u.DiscordUser)).AsNoTracking()
				 .ToArray();
	}
}