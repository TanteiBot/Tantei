// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2025 N0D4N

using PaperMalKing.Common.Exceptions;

namespace PaperMalKing.Startup.Exceptions;

public sealed class GuildManagementException(string message, ulong? guildId = null, ulong? channelId = null) : UserFacingException(message)
{
	public ulong? GuildId { get; } = guildId;

	public ulong? ChannelId { get; } = channelId;
}