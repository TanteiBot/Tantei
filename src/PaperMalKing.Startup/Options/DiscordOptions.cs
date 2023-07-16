// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.Collections.Generic;

namespace PaperMalKing.Startup.Options;

public sealed class DiscordOptions
{
	public const string Discord = "Discord";

	public required string Token { get; init; }

	public required string ClientId { get; init; }

	public required string ClientSecret { get; init; }

	public required IReadOnlyList<DiscordActivityOptions> Activities { get; init; }

	public sealed class DiscordActivityOptions
	{
		public required string ActivityType { get; init; }

		public required string PresenceText { get; init; }

		public required int TimeToBeDisplayedInMilliseconds { get; init; }

		public required string Status { get; init; }
	}
}