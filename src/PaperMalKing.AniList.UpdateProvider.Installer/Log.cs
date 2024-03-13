// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using Microsoft.Extensions.Logging;

namespace PaperMalKing.AniList.UpdateProvider.Installer;

internal static partial class Log
{
	[LoggerMessage(LogLevel.Trace, "Resetting AniList ratelimiter")]
	public static partial void ResettingRateLimiter(this ILogger<HeaderBasedRateLimitMessageHandler> logger);

	[LoggerMessage(LogLevel.Debug, "AniList exceeded rate-limit waiting {Delay}")]
	public static partial void RateLimitExceeded(this ILogger<HeaderBasedRateLimitMessageHandler> logger, long delay);

	[LoggerMessage(LogLevel.Information, "Got 429'd waiting {Delay}")]
	public static partial void Got429HttpResponse(this ILogger<HeaderBasedRateLimitMessageHandler> logger, TimeSpan delay);

	[LoggerMessage(LogLevel.Trace, "AniList rate limit remaining {RateLimitRemaining}")]
	public static partial void RateLimitRemaining(this ILogger<HeaderBasedRateLimitMessageHandler> logger, sbyte rateLimitRemaining);
}