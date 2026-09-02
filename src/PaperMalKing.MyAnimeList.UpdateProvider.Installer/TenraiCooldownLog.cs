// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using Microsoft.Extensions.Logging;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Installer;

internal static partial class TenraiCooldownLog
{
	[LoggerMessage(EventId = 1, Level = LogLevel.Warning, Message = "Tenrai shared Retry-After cooldown engaged for {RetryAfter}")]
	public static partial void TenraiCooldownEngaged(this ILogger logger, TimeSpan retryAfter);
}
