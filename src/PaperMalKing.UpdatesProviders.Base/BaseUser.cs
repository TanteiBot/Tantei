// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using PaperMalKing.Database.Models;

namespace PaperMalKing.UpdatesProviders.Base;

public sealed record BaseUser(string Username, DiscordUser? DiscordUser = null)
{
	public static BaseUser Empty { get; } = new("");

	public static BaseUser FromUsername(string? username, DiscordUser? discordUser = null)
	{
		return username is null ? Empty : new(username, discordUser);
	}
}