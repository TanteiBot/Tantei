// Tantei.
// Copyright (C) 2021-2023 N0D4N
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

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