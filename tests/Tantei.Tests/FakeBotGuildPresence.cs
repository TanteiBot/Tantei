// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using PaperMalKing.Startup.Web.Guilds;

namespace Tantei.Tests;

// Shared presence-axis double for InviteAuthorizationTests and GuildQueryServiceTests. The admin-axis
// variant in GuildAdminAuthorizationHandlerTests (throwing GetGuild guard + asserted CallCount) and the
// three CreatePrincipal helpers are intentionally kept separate: their differing shapes encode distinct
// intents, so folding them onto this double would blunt those tests. Do not "finish the job".
internal sealed class FakeBotGuildPresence(params ulong[] presentGuildIds) : IBotGuildPresence
{
	public BotGuildInfo? GetGuild(ulong guildId)
		=> presentGuildIds.Contains(guildId) ? new(guildId, $"Guild {guildId}", IconUrl: null) : null;

	public Task<bool> IsGuildAdminAsync(ulong guildId, ulong discordUserId) => Task.FromResult(false);
}
