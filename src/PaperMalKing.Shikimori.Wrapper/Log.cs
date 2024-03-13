// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using Microsoft.Extensions.Logging;
using PaperMalKing.Common.Enums;

namespace PaperMalKing.Shikimori.Wrapper;

internal static partial class Log
{
	[LoggerMessage(LogLevel.Debug, "Requesting {UserId} favourites")]
	public static partial void RequestingFavorites(this ILogger<ShikiClient> client, uint userId);

	[LoggerMessage(LogLevel.Debug, "Requesting {@UserId} history. Page {@Page}")]
	public static partial void RequestingHistoryPage(this ILogger<ShikiClient> logger, uint userId, uint page);

	[LoggerMessage(LogLevel.Debug, "Requesting media with id: {MediaId}, and type: {Type}")]
	public static partial void RequestingMedia(this ILogger<ShikiClient> logger, ulong mediaId, ListEntryType type);

	[LoggerMessage(LogLevel.Debug, "Requesting staff for media with id: {MediaId}, and type: {Type}")]
	public static partial void RequestingStaff(this ILogger<ShikiClient> logger, ulong mediaId, ListEntryType type);

	[LoggerMessage(LogLevel.Debug, "Requesting {UserId} info")]
	public static partial void RequestingUserInfo(this ILogger<ShikiClient> logger, uint userId);

	[LoggerMessage(LogLevel.Debug, "Requesting {Nickname} profile")]
	public static partial void RequestingUserInfo(this ILogger<ShikiClient> logger, string nickname);

	[LoggerMessage(LogLevel.Debug, "Requesting {UserId} achievements")]
	public static partial void RequestingUserAchievements(this ILogger<ShikiClient> logger, uint userId);
}