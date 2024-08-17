// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaperMalKing.Database;
using PaperMalKing.UpdatesProviders.Base;

namespace PaperMalKing.Startup.Services;

internal sealed class UserCleanupService
{
	private readonly ILogger<UserCleanupService> _logger;
	private readonly DiscordClient _discordClient;
	private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
	private readonly GeneralUserService _userService;

	public UserCleanupService(ILogger<UserCleanupService> logger, DiscordClient discordClient, IDbContextFactory<DatabaseContext> dbContextFactory, GeneralUserService userService)
	{
		this._logger = logger;
		this._discordClient = discordClient;
		this._dbContextFactory = dbContextFactory;
		this._userService = userService;
	}

	public async Task ExecuteCleanupAsync()
	{
		await using var db = this._dbContextFactory.CreateDbContext();
		this._logger.StartingUserCleanup();
		foreach (var discordUser in db.DiscordUsers.TagWith("Query users for cleanup").TagWithCallSite().Include(x => x.Guilds).AsNoTracking().ToArray())
		{
			var userId = discordUser.DiscordUserId;
			if (discordUser.Guilds is [])
			{
				await this._userService.RemoveUserIfInNoGuildsAsync(userId);
			}
			else
			{
				foreach (var guildId in discordUser.Guilds.Select(x => x.DiscordGuildId))
				{
					if (!this._discordClient.Guilds.TryGetValue(guildId, out var guild))
					{
						continue;
					}

					try
					{
						_ = await guild.GetMemberAsync(userId);
					}
					catch (NotFoundException)
					{
						await this._userService.RemoveUserInGuildAsync(guildId, userId);
					}
				}
			}
		}

		this._logger.FinishingUserCleanup();
	}
}