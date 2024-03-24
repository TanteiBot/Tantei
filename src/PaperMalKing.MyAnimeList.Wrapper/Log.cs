// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using Microsoft.Extensions.Logging;
using PaperMalKing.Common.Enums;

namespace PaperMalKing.MyAnimeList.Wrapper;

internal static partial class Log
{
	[LoggerMessage(LogLevel.Debug, "Requesting {@Username} profile")]
	public static partial void RequestingProfile(this ILogger<MyAnimeListClient> logger, string username);

	[LoggerMessage(LogLevel.Trace, "Starting parsing {Username} profile")]
	public static partial void StartingParsingProfile(this ILogger<MyAnimeListClient> logger, string username);

	[LoggerMessage(LogLevel.Trace, "Ended parsing {Username} profile")]
	public static partial void EndingParsingProfile(this ILogger<MyAnimeListClient> logger, string username);

	[LoggerMessage(LogLevel.Debug, "Requesting username by id {Id}")]
	public static partial void RequestingUsername(this ILogger<MyAnimeListClient> logger, uint id);

	[LoggerMessage(LogLevel.Debug, "Requesting {Username} {@Type} list")]
	public static partial void RequestingList(this ILogger<MyAnimeListClient> logger, string username, ListEntryType type);

	[LoggerMessage(LogLevel.Debug, "Requesting {Id} anime details")]
	public static partial void RequestingAnimeDetails(this ILogger<MyAnimeListClient> logger, long id);

	[LoggerMessage(LogLevel.Warning, "Error happened in Jikan when requesting anime {Id}")]
	public static partial void ErrorHappenedInJikanWhenRequestingAnime(this ILogger<MyAnimeListClient> logger, Exception ex, long id);

	[LoggerMessage(LogLevel.Debug, "Requesting {Id} manga details")]
	public static partial void RequestingMangaDetails(this ILogger<MyAnimeListClient> logger, long id);

	[LoggerMessage(LogLevel.Warning, "Error happened in Jikan when requesting manga {Id}")]
	public static partial void ErrorHappenedInJikanWhenRequestingManga(this ILogger<MyAnimeListClient> logger, Exception ex, long id);

	[LoggerMessage(LogLevel.Debug, "Requesting {Id} anime seiyu")]
	public static partial void RequestingSeyuDetails(this ILogger<MyAnimeListClient> logger, long id);

	[LoggerMessage(LogLevel.Warning, "Error happened in Jikan when requesting seyu {Id}")]
	public static partial void ErrorHappenedInJikanWhenRequestingSeyu(this ILogger<MyAnimeListClient> logger, Exception ex, long id);
}