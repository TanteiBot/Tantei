﻿// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;

namespace PaperMalKing.Startup.Exceptions;

public sealed class GuildManagementException : Exception
{
	public ulong? GuildId { get; }

	public ulong? ChannelId { get; }

	public GuildManagementException(string message, ulong? guildId = null, ulong? channelId = null)
		: base(message)
	{
		this.GuildId = guildId;
		this.ChannelId = channelId;
	}
}