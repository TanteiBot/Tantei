// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using Microsoft.EntityFrameworkCore;
using PaperMalKing.Database;

namespace PaperMalKing.Startup.Web.Guilds;

public sealed record ManageableGuild(ulong GuildId, string Name, string? IconUrl);

public sealed class GuildQueryService(IDbContextFactory<DatabaseContext> _dbContextFactory, IBotGuildPresence _botGuildPresence, UserGuildsCache _userGuildsCache)
{
	public async Task<IReadOnlyList<ManageableGuild>> GetManageableGuildsAsync(ulong discordUserId, CancellationToken cancellationToken)
	{
		await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);
		var user = db.GetDiscordUserById(discordUserId);
		if (user is null)
		{
			return [];
		}

		var result = new List<ManageableGuild>(user.Guilds.Count);
		foreach (var guild in user.Guilds)
		{
			if (_botGuildPresence.GetGuild(guild.DiscordGuildId) is { } info)
			{
				result.Add(new(info.GuildId, info.Name, info.IconUrl));
			}
		}

		return result;
	}
}
