// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using PaperMalKing.Database.Models;

namespace PaperMalKing.UpdatesProviders.Base;

public sealed class BaseUser
{
	public static BaseUser Empty { get; } = new BaseUser("");

	public string Username { get; }

	public DiscordUser? DiscordUser { get; }

	public static BaseUser FromUsername(string? username, DiscordUser? discordUser = null)
	{
		return username is null ? Empty : new BaseUser(username, discordUser);
	}

	public BaseUser(string username, DiscordUser? discordUser = null)
	{
		this.Username = username;
		this.DiscordUser = discordUser;
	}
}