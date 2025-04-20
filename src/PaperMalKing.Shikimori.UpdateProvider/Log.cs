// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2025 N0D4N

using Microsoft.Extensions.Logging;
using PaperMalKing.Shikimori.UpdateProvider.Achievements;

namespace PaperMalKing.Shikimori.UpdateProvider;

internal static partial class Log
{
	[LoggerMessage(LogLevel.Warning, "No achievements found")]
	public static partial void DidntFindAnyAchievements(this ILogger<ShikiAchievementsService> logger);

	[LoggerMessage(LogLevel.Information, "Found {amount} achievements")]
	public static partial void FoundAchievements(this ILogger<ShikiAchievementsService> logger, int amount);
}