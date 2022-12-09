// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PaperMalKing.Database;
using PaperMalKing.Database.Models;
using PaperMalKing.Database.Models.Shikimori;
using PaperMalKing.Shikimori.Wrapper;
using PaperMalKing.Shikimori.Wrapper.Models;
using PaperMalKing.UpdatesProviders.Base;
using PaperMalKing.UpdatesProviders.Base.Exceptions;

namespace PaperMalKing.Shikimori.UpdateProvider;

public sealed class ShikiUserService : IUpdateProviderUserService
{
	private readonly ShikiClient _client;
	private readonly ILogger<ShikiUserService> _logger;
	private readonly IServiceProvider _serviceProvider;

	public static string Name => Constants.NAME;

	public ShikiUserService(ShikiClient client, ILogger<ShikiUserService> logger, IServiceProvider serviceProvider)
	{
		this._client = client;
		this._logger = logger;
		this._serviceProvider = serviceProvider;
	}

	/// <inheritdoc />
	public async Task<BaseUser> AddUserAsync(string username, ulong userId, ulong guildId)
	{
		using var scope = this._serviceProvider.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
		var dbUser = db.ShikiUsers.Include(su => su.DiscordUser).ThenInclude(du => du.Guilds).FirstOrDefault(su => su.DiscordUserId == userId);
		DiscordGuild? guild;
		if (dbUser != null) // User already in db
		{
			if (dbUser.DiscordUser.Guilds.Any(g => g.DiscordGuildId == guildId))
				throw new UserProcessingException(
					"You already have your account connected. If you want to switch to another account, remove current one, then add the new one.");
			guild = db.DiscordGuilds.FirstOrDefault(g => g.DiscordGuildId == guildId);
			if (guild == null)
				throw new UserProcessingException(new(username),
					"Current server is not in database, ask server administrator to add this server to bot");

			dbUser.DiscordUser.Guilds.Add(guild);
			db.ShikiUsers.Update(dbUser);
			await db.SaveChangesAndThrowOnNoneAsync().ConfigureAwait(false);
			return new(username);
		}

		guild =  db.DiscordGuilds.FirstOrDefault(g => g.DiscordGuildId == guildId);
		if (guild == null)
			throw new UserProcessingException(new(username),
				"Current server is not in database, ask server administrator to add this server to bot");
		var dUser = db.DiscordUsers.Include(x=>x.Guilds).FirstOrDefault(du => du.DiscordUserId == userId);
		var shikiUser = await this._client.GetUserAsync(username).ConfigureAwait(false);
		var history = await this._client.GetUserHistoryAsync(shikiUser.Id, 1, 1, HistoryRequestOptions.Any).ConfigureAwait(false);
		var favourites = await this._client.GetUserFavouritesAsync(shikiUser.Id).ConfigureAwait(false);
		if (dUser == null)
		{
			dUser = new()
			{
				BotUser = new(),
				Guilds = new DiscordGuild[1] { guild },
				DiscordUserId = userId,
			};
		}
		else if(dUser.Guilds.All(x => x.DiscordGuildId != guildId))
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
			LastHistoryEntryId = history.Data.Max(he => he.Id)
		};
		dbUser.Favourites.ForEach(f => f.User = dbUser);
		db.ShikiUsers.Add(dbUser);
		await db.SaveChangesAndThrowOnNoneAsync().ConfigureAwait(false);
		return new(username);
	}

	/// <inheritdoc />
	public async Task<BaseUser> RemoveUserAsync(ulong userId)
	{
		using var scope = this._serviceProvider.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
		var user = db.ShikiUsers.Include(su => su.DiscordUser).ThenInclude(du => du.Guilds).FirstOrDefault(su => su.DiscordUserId == userId);
		if (user == null)
			throw new UserProcessingException($"You weren't tracked by {Name} update checker");

		db.ShikiUsers.Remove(user);
		await db.SaveChangesAndThrowOnNoneAsync().ConfigureAwait(false);
		return new("");
	}

	public async Task RemoveUserHereAsync(ulong userId, ulong guildId)
	{
		using var scope = this._serviceProvider.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
		var user = db.DiscordUsers.Include(du => du.Guilds).FirstOrDefault(du => du.DiscordUserId == userId);
		if (user == null)
			throw new UserProcessingException("You weren't registered in bot");
		var guild = user.Guilds.FirstOrDefault(g => g.DiscordGuildId == guildId);
		if (guild == null)
			throw new UserProcessingException("You weren't registered in this server");

		user.Guilds.Remove(guild);
		db.DiscordUsers.Update(user);
		await db.SaveChangesAndThrowOnNoneAsync().ConfigureAwait(false);
	}

	/// <inheritdoc />
	public IAsyncEnumerable<BaseUser> ListUsersAsync(ulong guildId)
	{
		using var scope = this._serviceProvider.CreateScope();
		var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
		return db.ShikiUsers.Include(su => su.DiscordUser).ThenInclude(du => du.Guilds).Select(su => new
				 {
					 su.DiscordUser,
					 su.LastHistoryEntryId
				 }).Where(u => u.DiscordUser.Guilds.Any(g => g.DiscordGuildId == guildId)).OrderByDescending(u => u.LastHistoryEntryId)
				 .Select(u => new BaseUser("", u.DiscordUser)).AsNoTracking().AsAsyncEnumerable();
	}
}