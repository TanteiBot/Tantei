// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

namespace PaperMalKing.Database.Models;

public interface IUpdateProviderUser
{
	public ulong DiscordUserId { get; init; }
	public DiscordUser DiscordUser { get; init; }

}