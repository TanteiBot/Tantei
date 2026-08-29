// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using DSharpPlus;
using Microsoft.EntityFrameworkCore;
using PaperMalKing.Database;

namespace PaperMalKing.Startup.Web.Guilds;

public sealed class GuildQueryService(IDbContextFactory<DatabaseContext> _dbContextFactory, IBotGuildPresence _botGuildPresence)
{
#pragma warning disable CA1859 // Use concrete types when possible for improved performance
	public IReadOnlyList<ManageableGuild> GetManageableGuilds(ulong discordUserId)
#pragma warning restore CA1859
	{
		using var db = _dbContextFactory.CreateDbContext();
		var user = db.GetDiscordUserById(discordUserId);
		if (user is null)
		{
			return [];
		}

		return [..user.Guilds.Select(x => _botGuildPresence.GetGuild(x.DiscordGuildId))
				   .Where(x => x is not null).Select(x => new ManageableGuild(x!.GuildId, x.Name, x.IconUrl))];
	}

	public IReadOnlyList<InvitableGuild> GetInvitableGuilds(IReadOnlyList<DiscordPartialGuild> guilds)
		=> [.. guilds.Where(guild => guild.Permissions.HasFlag(Permissions.ManageGuild) && _botGuildPresence.GetGuild(guild.Id) is null)
					 .Select(guild => new InvitableGuild(guild.Id, guild.Name, guild.IconUrl))];
}
