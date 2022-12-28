// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using DSharpPlus.Entities;

namespace PaperMalKing.UpdatesProviders.MyAnimeList;

internal static class Constants
{
	internal const string Name = "MyAnimeList";

	internal const string OfficialApiHttpClientName = "OfficialApiMyAnimeList";

	internal const string UnOfficialApiHttpClientName = $"Un{OfficialApiHttpClientName}";

	internal const string OfficialApiHeaderName = "X-MAL-CLIENT-ID";

	internal static readonly DiscordColor MalGreen = new("#2db039");

	internal static readonly DiscordColor MalBlue = new("#26448f");

	internal static readonly DiscordColor MalYellow = new("#f9d457");

	internal static readonly DiscordColor MalRed = new("#a12f31");

	internal static readonly DiscordColor MalGrey = new("#c3c3c3");

	internal static readonly DiscordColor MalBlack = DiscordColor.NotQuiteBlack;
}