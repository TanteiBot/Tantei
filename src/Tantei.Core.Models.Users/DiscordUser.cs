// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

namespace Tantei.Core.Models.Users;

public sealed record DiscordUser(ulong Id, BotUser BotUser)
{
	public ulong BotUserId { get; init; }

	public IList<DiscordGuild> Guilds { get; init; } = Array.Empty<DiscordGuild>();
}