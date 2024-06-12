// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.RateLimiting;

namespace PaperMalKing.Common.RateLimiters;

public static class RateLimiterFactory
{
	[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Its candled in created ratelimiter")]
	public static RateLimiter<T> Create<T>(RateLimitValue rateLimitValue)
	{
		ArgumentNullException.ThrowIfNull(rateLimitValue);
		if (rateLimitValue.AmountOfRequests == 0 || rateLimitValue.PeriodInMilliseconds == 0)
		{
			return new RateLimiter<T>(NullRateLimiter.Instance);
		}

		const int queueLimit = 200;

		return new RateLimiter<T>(new FixedWindowRateLimiter(new()
		{
			Window = TimeSpan.FromMilliseconds(rateLimitValue.PeriodInMilliseconds),
			AutoReplenishment = true,
			PermitLimit = rateLimitValue.AmountOfRequests,
			QueueLimit = queueLimit,
			QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
		}));
	}
}