// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System.Collections.Generic;

namespace PaperMalKing.Options;

public sealed class DiscordOptions
{
	public const string Discord = "Discord";

	public required string Token { get; set; }

	public required IReadOnlyList<DiscordActivityOptions> Activities { get; set; }

	public sealed class DiscordActivityOptions
	{
		public required string ActivityType { get; set; }

		public required string PresenceText { get; set; }

		public required int TimeToBeDisplayedInMilliseconds { get; set; }

		public required string Status { get; set; }
	}
}