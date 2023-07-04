// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

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

	public UserCleanupService(ILogger<UserCleanupService> logger, DiscordClient discordClient, IDbContextFactory<DatabaseContext> dbContextFactory,
							  GeneralUserService userService)
	{
		this._logger = logger;
		this._discordClient = discordClient;
		this._dbContextFactory = dbContextFactory;
		this._userService = userService;
	}

	public async Task ExecuteCleanupAsync()
	{
		using var db = this._dbContextFactory.CreateDbContext();
		this._logger.LogInformation("Starting to look for users without links to any guild, or users that left server while bot was offline");
		foreach (var discordUser in db.DiscordUsers.Include(x => x.Guilds).AsNoTracking().ToArray())
		{
			var userId = discordUser.DiscordUserId;
			if (discordUser.Guilds.Count == 0)
			{
				await this._userService.RemoveUserIfInNoGuildsAsync(userId).ConfigureAwait(false);
			}
			else
			{
				foreach (var guildId in discordUser.Guilds.Select(x => x.DiscordGuildId))
				{
					if (this._discordClient.Guilds.TryGetValue(guildId, out var guild))
					{
						try
						{
							_ = await guild.GetMemberAsync(userId).ConfigureAwait(false);
						}
						#pragma warning disable CA1031
						catch (NotFoundException)
							#pragma warning restore CA1031
						{
							await this._userService.RemoveUserInGuildAsync(guildId, userId).ConfigureAwait(false);
						}
					}
				}
			}
		}

		this._logger.LogInformation("Finishing looking for users without links to any guild");
	}
}