// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaperMalKing.Database;
using PaperMalKing.UpdatesProviders.Base.Exceptions;

namespace PaperMalKing.UpdatesProviders.Base;

public sealed class GeneralUserService
{
	private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
	private readonly ILogger<GeneralUserService> _logger;

	public GeneralUserService(IDbContextFactory<DatabaseContext> dbContextFactory, ILogger<GeneralUserService> logger)
	{
		this._dbContextFactory = dbContextFactory;
		this._logger = logger;
	}

	public async Task RemoveUserInGuildAsync(ulong guildId, ulong userId)
	{
		await using var db = this._dbContextFactory.CreateDbContext();
		var guild = db.DiscordGuilds.TagWith("Query user to remove him in a guild").TagWithCallSite().Include(g => g.Users)
					  .First(g => g.DiscordGuildId == guildId);
		var user = guild.Users.FirstOrDefault(u => u.DiscordUserId == userId) ?? throw new UserProcessingException("Such user wasn't found as registered in this guild");
		this._logger.RemovingUser(user);
		guild.Users.Remove(user);
		await db.SaveChangesAndThrowOnNoneAsync();
	}

	public async Task RemoveUserIfInNoGuildsAsync(ulong userId)
	{
		await using var db = this._dbContextFactory.CreateDbContext();
		this._logger.TryToRemoveUserWithNoGuilds(userId);
		var user = db.DiscordUsers.TagWith("Query user to remove him from guild").TagWithCallSite().Include(x => x.Guilds).Include(x => x.BotUser)
					 .FirstOrDefault(x => x.DiscordUserId == userId) ?? throw new UserProcessingException($"User with id {userId} wasn't found");
		if (user.Guilds is not [])
		{
			this._logger.SkipRemovingUserWithGuilds(user);
			return;
		}

		db.MalUsers.Where(mu => mu.DiscordUserId == user.DiscordUserId).ExecuteDelete();
		db.ShikiUsers.Where(mu => mu.DiscordUserId == user.DiscordUserId).ExecuteDelete();
		db.AniListUsers.Where(mu => mu.DiscordUserId == user.DiscordUserId).ExecuteDelete();
		db.DiscordUsers.Where(x => x.DiscordUserId == userId).ExecuteDelete();
		db.BotUsers.Where(bu => bu.UserId == user.BotUser.UserId).ExecuteDelete();
		this._logger.RemovingUserWithNoGuilds(userId);
	}
}