// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System.Collections.Generic;

namespace PaperMalKing.Options
{
	public sealed class DiscordOptions
	{
		public const string Discord = "Discord";

		public string Token { get; set; } = null!;

		public IReadOnlyList<DiscordActivityOptions> Activities { get; set; } = null!;

		#pragma warning disable CA1034
		public sealed class DiscordActivityOptions
		#pragma warning restore CA1034
		{
			public string ActivityType { get; set; } = null!;

			public string PresenceText { get; set; } = null!;

			public int TimeToBeDisplayedInMilliseconds { get; set; }

			public string Status { get; set; } = null!;
		}
	}
}