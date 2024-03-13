// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

namespace PaperMalKing.Common.RateLimiters;

internal sealed class NullRateLimiter : RateLimiter
{
	private static readonly RateLimiterStatistics Statistics = new()
	{
		CurrentAvailablePermits = 0,
		CurrentQueuedCount = 0,
		TotalFailedLeases = 0,
		TotalSuccessfulLeases = 0,
	};

	public static NullRateLimiter Instance { get; } = new();

	private NullRateLimiter()
	{
	}

	public override RateLimiterStatistics GetStatistics() => Statistics;

	protected override RateLimitLease AttemptAcquireCore(int permitCount) => NullRateLimitLease.Instance;

	protected override ValueTask<RateLimitLease> AcquireAsyncCore(int permitCount, CancellationToken cancellationToken) =>
		ValueTask.FromResult((RateLimitLease)NullRateLimitLease.Instance);

	public override TimeSpan? IdleDuration => TimeSpan.Zero;
}

[SuppressMessage("Style", "IDE0036:Order modifiers", Justification = "Visibility should appear before sealed")]
file sealed class NullRateLimitLease : RateLimitLease
{
	public static readonly NullRateLimitLease Instance = new();

	private NullRateLimitLease()
	{
	}

	public override bool TryGetMetadata(string metadataName, out object? metadata)
	{
		metadata = null;
		return false;
	}

	public override bool IsAcquired => true;

	public override IEnumerable<string> MetadataNames => [];
}