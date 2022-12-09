// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using PaperMalKing.Database.Models;

namespace PaperMalKing.UpdatesProviders.Base;

public class BaseUser
{
	public string Username { get; init; }

	public DiscordUser? DiscordUser { get; init; }

	public BaseUser(string username, DiscordUser? discordUser = null)
	{
		this.Username = username;
		this.DiscordUser = discordUser;
	}
}