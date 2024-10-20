﻿// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using DSharpPlus.Entities;

namespace PaperMalKing.MyAnimeList.UpdateProvider;

internal static class Constants
{
	public const string Name = "MyAnimeList";

	public const string OfficialApiHttpClientName = "OfficialApiMyAnimeList";

	public const string UnOfficialApiHttpClientName = $"Un{OfficialApiHttpClientName}";

	public const string JikanHttpClientName = "Jikan";

	public const string OfficialApiHeaderName = "X-MAL-CLIENT-ID";

	public const string JikanApiUrl = "https://api.jikan.moe/v4/";

	public const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/129.0.0.0 Safari/537.36";

	/// <summary>
	/// Discord doesn't support .ico formats in footer icons.
	/// </summary>
	public const string FavIcon = "https://cdn.myanimelist.net/images/MalAppIcon_180px.png";

	public static readonly DiscordColor MalGreen = new("#2db039");

	public static readonly DiscordColor MalBlue = new("#26448f");

	public static readonly DiscordColor MalYellow = new("#f9d457");

	public static readonly DiscordColor MalRed = new("#a12f31");

	public static readonly DiscordColor MalGrey = new("#c3c3c3");
}