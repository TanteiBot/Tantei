// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using System.Threading;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

namespace PaperMalKing.Common.RateLimiters;

public sealed class RateLimiter<T> : RateLimiter
{
	private readonly RateLimiter _rateLimiterImplementation;

	public RateLimiter(RateLimiter rateLimiterImplementation)
	{
		this._rateLimiterImplementation = rateLimiterImplementation;
	}

	public override RateLimiterStatistics? GetStatistics() => this._rateLimiterImplementation.GetStatistics();

	protected override RateLimitLease AttemptAcquireCore(int permitCount) => this._rateLimiterImplementation.AttemptAcquire(permitCount);

	protected override ValueTask<RateLimitLease> AcquireAsyncCore(int permitCount, CancellationToken cancellationToken) =>
		this._rateLimiterImplementation.AcquireAsync(permitCount, cancellationToken);

	public override TimeSpan? IdleDuration => this._rateLimiterImplementation.IdleDuration;
}