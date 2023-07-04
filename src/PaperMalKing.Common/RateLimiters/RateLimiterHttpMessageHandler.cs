// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

namespace PaperMalKing.Common.RateLimiters;

public sealed class RateLimiterHttpMessageHandler : DelegatingHandler
{
	public RateLimiter RateLimiter { get; }

	internal RateLimiterHttpMessageHandler(RateLimiter rateLimiter)
	{
		this.RateLimiter = rateLimiter;
	}

	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		while (!cancellationToken.IsCancellationRequested)
		{
			using (var rateLimitLease = await this.RateLimiter.AcquireAsync(1, cancellationToken).ConfigureAwait(false))
			{
				if (rateLimitLease.IsAcquired)
				{
					return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
				}
			}
		}

		cancellationToken.ThrowIfCancellationRequested();
		throw new UnreachableException();
	}

	protected override void Dispose(bool disposing)
	{
		this.RateLimiter.Dispose();
		base.Dispose(disposing);
	}
}