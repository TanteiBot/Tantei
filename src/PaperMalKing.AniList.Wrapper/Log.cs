// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using Microsoft.Extensions.Logging;

namespace PaperMalKing.AniList.Wrapper;

internal static partial class Log
{
	[LoggerMessage(LogLevel.Debug, "Requesting initial info for {Username}, {Page}")]
	public static partial void RequestingInitialInfo(this ILogger<AniListClient> logger, string username, byte page);

	[LoggerMessage(LogLevel.Debug, "Requesting updates check for {UserId}, {Page}")]
	public static partial void RequestingUpdatesCheck(this ILogger<AniListClient> logger, uint userId, byte page);
}