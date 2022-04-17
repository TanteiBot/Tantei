// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

namespace Tantei.Core.Models.Users;

public sealed record DiscordGuild(ulong Id, ulong PostingChannelId)
{
	public IList<DiscordUser> Users { get; init; } = Array.Empty<DiscordUser>();
}