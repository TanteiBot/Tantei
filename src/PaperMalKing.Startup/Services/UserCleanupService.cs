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

internal sealed class UserCleanupService(ILogger<UserCleanupService> _logger, DiscordClient _discordClient, IDbContextFactory<DatabaseContext> _dbContextFactory, GeneralUserService _userService)
{
	public async Task ExecuteCleanupAsync()
	{
		await using var db = _dbContextFactory.CreateDbContext();
		_logger.StartingUserCleanup();
		foreach (var discordUser in db.DiscordUsers.TagWith("Query users for cleanup").TagWithCallSite().Include(x => x.Guilds).AsNoTracking().ToArray())
		{
			var userId = discordUser.DiscordUserId;
			if (discordUser.Guilds is [])
			{
				await _userService.RemoveUserIfInNoGuildsAsync(userId);
			}
			else
			{
				foreach (var guildId in discordUser.Guilds.Select(x => x.DiscordGuildId))
				{
					if (!_discordClient.Guilds.TryGetValue(guildId, out var guild))
					{
						continue;
					}

					try
					{
						_ = await guild.GetMemberAsync(userId);
					}
					catch (NotFoundException)
					{
						await _userService.RemoveUserInGuildAsync(guildId, userId);
					}
				}
			}
		}

		_logger.FinishingUserCleanup();
	}
}