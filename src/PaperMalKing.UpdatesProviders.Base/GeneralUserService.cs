// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

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
		using var db = this._dbContextFactory.CreateDbContext();
		var guild = db.DiscordGuilds.TagWith("Query user to remove him in a guild").TagWithCallSite().Include(g => g.Users)
					  .First(g => g.DiscordGuildId == guildId);
		var user = guild.Users.FirstOrDefault(u => u.DiscordUserId == userId) ?? throw new UserProcessingException("Such user wasn't found as registered in this guild");
		this._logger.LogInformation("Removing {User}", user);
		guild.Users.Remove(user);
		await db.SaveChangesAndThrowOnNoneAsync().ConfigureAwait(false);
	}

	public async Task RemoveUserIfInNoGuildsAsync(ulong userId)
	{
		using var db = this._dbContextFactory.CreateDbContext();
		this._logger.LogInformation("Trying to remove user with {Id} if he has no guilds linked", userId);
		var user = db.DiscordUsers.TagWith("Query user to remove him from guild").TagWithCallSite().Include(x => x.Guilds).Include(x => x.BotUser).FirstOrDefault(x => x.DiscordUserId == userId) ?? throw new UserProcessingException($"User with id {userId} wasnt found");
		if (user.Guilds.Count != 0)
		{
			this._logger.LogInformation("{User} is tracked in some guilds. Skip deleting it", user);
			return;
		}

		db.BotUsers.Remove(user.BotUser);
		this._logger.LogInformation("Removing user with {Id} because he has no guilds linked", userId);
		await db.SaveChangesAndThrowOnNoneAsync().ConfigureAwait(false);
	}
}