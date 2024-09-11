// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;

namespace PaperMalKing.Startup.Exceptions;

public sealed class GuildManagementException(string message, ulong? guildId = null, ulong? channelId = null) : Exception(message)
{
	public ulong? GuildId { get; } = guildId;

	public ulong? ChannelId { get; } = channelId;
}