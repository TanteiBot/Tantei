// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using DSharpPlus;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Logging;

namespace PaperMalKing.Startup.Web.Guilds;

public sealed class BotGuildPresence(DiscordClient _discordClient, ILogger<BotGuildPresence> _logger) : IBotGuildPresence
{
	public BotGuildInfo? GetGuild(ulong guildId)
		=> _discordClient.Guilds.TryGetValue(guildId, out var guild) ? new(guild.Id, guild.Name, guild.IconUrl) : null;

	public async Task<bool> IsGuildAdminAsync(ulong guildId, ulong discordUserId)
	{
		if (!_discordClient.Guilds.TryGetValue(guildId, out var guild))
		{
			return false;
		}

		try
		{
			var member = await guild.GetMemberAsync(discordUserId);
			return member.Permissions.HasFlag(Permissions.ManageGuild);
		}
		catch (DiscordException ex)
		{
			_logger.FailedToCheckGuildAdminStatus(ex, guildId, discordUserId);
			return false;
		}
	}
}
