// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using PaperMalKing.Database.Models;

namespace PaperMalKing.UpdatesProviders.Base;

public sealed class UpdateFoundEventArgs : EventArgs
{
	public IUpdate Update { get; }

	public DiscordUser DiscordUser { get; }

	public UpdateFoundEventArgs(IUpdate update, DiscordUser discordUser)
	{
		this.Update = update;
		this.DiscordUser = discordUser;
	}
}