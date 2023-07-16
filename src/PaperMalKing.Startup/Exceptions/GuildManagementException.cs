// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using System.Diagnostics.CodeAnalysis;

namespace PaperMalKing.Startup.Exceptions;

[SuppressMessage("Design", "CA1064:Exceptions should be public")]
[SuppressMessage("Critical Code Smell", "S3871:Exception types should be \"public\"")]
internal sealed class GuildManagementException : Exception
{
	public ulong? GuildId { get; }

	public ulong? ChannelId { get; }

	public GuildManagementException(string message, ulong? guildId = default, ulong? channelId = default) : base(message)
	{
		this.GuildId = guildId;
		this.ChannelId = channelId;
	}
}