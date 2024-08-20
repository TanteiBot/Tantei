// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Threading;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

namespace PaperMalKing.Common.RateLimiters;

public sealed class RateLimiter<T>(RateLimiter _rateLimiterImplementation) : RateLimiter
{
	public override RateLimiterStatistics? GetStatistics() => _rateLimiterImplementation.GetStatistics();

	protected override RateLimitLease AttemptAcquireCore(int permitCount) => _rateLimiterImplementation.AttemptAcquire(permitCount);

	protected override ValueTask<RateLimitLease> AcquireAsyncCore(int permitCount, CancellationToken cancellationToken) =>
		_rateLimiterImplementation.AcquireAsync(permitCount, cancellationToken);

	public override TimeSpan? IdleDuration => _rateLimiterImplementation.IdleDuration;

	protected override void Dispose(bool disposing)
	{
		_rateLimiterImplementation.Dispose();
		base.Dispose(disposing);
	}
}