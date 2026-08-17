// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

namespace PaperMalKing.Startup.Web.Guilds;

public sealed record BotGuildInfo(ulong GuildId, string Name, string? IconUrl);

public interface IBotGuildPresence
{
	BotGuildInfo? GetGuild(ulong guildId);

	Task<bool> IsGuildAdminAsync(ulong guildId, ulong discordUserId);
}
