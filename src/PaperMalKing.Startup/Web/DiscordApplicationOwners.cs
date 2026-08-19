// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using DSharpPlus;

namespace PaperMalKing.Startup.Web;

public sealed class DiscordApplicationOwners(DiscordClient _discordClient) : IApplicationOwners
{
	public bool IsOwner(ulong discordUserId) => _discordClient.CurrentApplication?.Owners?.Any(o => o.Id == discordUserId) == true;
}
