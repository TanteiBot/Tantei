// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using Microsoft.Extensions.Logging;
using PaperMalKing.Common.Enums;

namespace PaperMalKing.MyAnimeList.Wrapper;

internal static partial class Log
{
	[LoggerMessage(LogLevel.Debug, "Requesting {Username} profile")]
	public static partial void RequestingProfile(this ILogger<MyAnimeListClient> logger, string username);

	[LoggerMessage(LogLevel.Trace, "Starting parsing {Username} profile")]
	public static partial void StartingParsingProfile(this ILogger<MyAnimeListClient> logger, string username);

	[LoggerMessage(LogLevel.Trace, "Ended parsing {Username} profile")]
	public static partial void EndingParsingProfile(this ILogger<MyAnimeListClient> logger, string username);

	[LoggerMessage(LogLevel.Debug, "Requesting username by id {Id}")]
	public static partial void RequestingUsername(this ILogger<MyAnimeListClient> logger, uint id);

	[LoggerMessage(LogLevel.Debug, "Requesting {Username} {Type} list")]
	public static partial void RequestingList(this ILogger<MyAnimeListClient> logger, string username, ListEntryType type);
}