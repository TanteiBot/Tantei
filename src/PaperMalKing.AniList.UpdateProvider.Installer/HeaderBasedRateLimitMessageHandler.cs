// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PaperMalKing.AniList.UpdateProvider.Installer;

internal sealed class HeaderBasedRateLimitMessageHandler(ILogger<HeaderBasedRateLimitMessageHandler> _logger) : DelegatingHandler
{
	private const sbyte RateLimitMaxRequests = 90;
	private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
	private sbyte _rateLimitRemaining = RateLimitMaxRequests;
	private long _timestamp = TimeProvider.System.GetUtcNow().ToUnixTimeSeconds();

	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		await this._semaphoreSlim.WaitAsync(cancellationToken);
		try
		{
			HttpResponseMessage response;
			do
			{
				var now = TimeProvider.System.GetUtcNow().ToUnixTimeSeconds();
				const int secondsInMinute = 60;
				if (now - this._timestamp >= secondsInMinute)
				{
					_logger.ResettingRateLimiter();
					this._timestamp = now;
					this._rateLimitRemaining = RateLimitMaxRequests;
				}

				this._rateLimitRemaining--;
				if (this._rateLimitRemaining < 0)
				{
					var delay = this._timestamp + 60 - now;
					_logger.RateLimitExceeded(delay);
					await Task.Delay(TimeSpan.FromSeconds(Math.Min(delay, secondsInMinute)), cancellationToken);
					this._timestamp = TimeProvider.System.GetUtcNow().ToUnixTimeSeconds();
					this._rateLimitRemaining = RateLimitMaxRequests;
				}

				response = await base.SendAsync(request, cancellationToken);

				if (response is { StatusCode: HttpStatusCode.TooManyRequests, Headers.RetryAfter.Delta: { } })
				{
					var delay = response.Headers.RetryAfter.Delta.Value.Add(TimeSpan.FromSeconds(1));
					_logger.Got429HttpResponse(delay);
					await Task.Delay(delay, cancellationToken);
				}
				else
				{
					this._rateLimitRemaining = sbyte.Parse(response.Headers.GetValues("X-RateLimit-Remaining").First(), NumberFormatInfo.InvariantInfo);
					_logger.RateLimitRemaining(this._rateLimitRemaining);
				}
			}
			while (!cancellationToken.IsCancellationRequested && response.StatusCode == HttpStatusCode.TooManyRequests);

			return response;
		}
		finally
		{
			this._semaphoreSlim.Release();
		}
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (disposing)
		{
			this._semaphoreSlim.Dispose();
		}
	}
}