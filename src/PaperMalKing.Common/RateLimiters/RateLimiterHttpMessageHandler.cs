// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

namespace PaperMalKing.Common.RateLimiters;

public sealed class RateLimiterHttpMessageHandler(RateLimiter _rateLimiter) : DelegatingHandler
{
	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		while (!cancellationToken.IsCancellationRequested)
		{
			using var rateLimitLease = await _rateLimiter.AcquireAsync(1, cancellationToken);
			if (rateLimitLease.IsAcquired)
			{
				return await base.SendAsync(request, cancellationToken);
			}
		}

		cancellationToken.ThrowIfCancellationRequested();
		throw new UnreachableException();
	}

	protected override void Dispose(bool disposing)
	{
		_rateLimiter.Dispose();
		base.Dispose(disposing);
	}
}