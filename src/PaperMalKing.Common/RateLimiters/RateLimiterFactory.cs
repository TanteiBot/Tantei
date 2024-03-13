// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using System.Threading.RateLimiting;

namespace PaperMalKing.Common.RateLimiters;

public static class RateLimiterFactory
{
	public static RateLimiter<T> Create<T>(RateLimitValue rateLimitValue)
	{
		ArgumentNullException.ThrowIfNull(rateLimitValue);
		if (rateLimitValue.AmountOfRequests == 0 || rateLimitValue.PeriodInMilliseconds == 0)
		{
			return new RateLimiter<T>(NullRateLimiter.Instance);
		}

		return new RateLimiter<T>(new FixedWindowRateLimiter(new()
		{
			Window = TimeSpan.FromMilliseconds(rateLimitValue.PeriodInMilliseconds),
			AutoReplenishment = true,
			PermitLimit = rateLimitValue.AmountOfRequests,
			QueueLimit = 200,
			QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
		}));
	}
}