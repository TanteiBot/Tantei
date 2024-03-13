// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PaperMalKing.AniList.UpdateProvider.Installer;

internal sealed class HeaderBasedRateLimitMessageHandler : DelegatingHandler
{
	private const sbyte RateLimitMaxRequests = 90;
	private readonly ILogger<HeaderBasedRateLimitMessageHandler> _logger;
	private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);
	private sbyte _rateLimitRemaining;
	private long _timestamp;

	public HeaderBasedRateLimitMessageHandler(ILogger<HeaderBasedRateLimitMessageHandler> logger)
	{
		this._logger = logger;
		this._rateLimitRemaining = RateLimitMaxRequests;
		this._timestamp = TimeProvider.System.GetUtcNow().ToUnixTimeSeconds();
	}

	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		await this._semaphoreSlim.WaitAsync(cancellationToken);
		try
		{
			HttpResponseMessage response;
			do
			{
				var now = TimeProvider.System.GetUtcNow().ToUnixTimeSeconds();
				if (now - this._timestamp >= 60)
				{
					this._logger.ResettingRateLimiter();
					this._timestamp = now;
					this._rateLimitRemaining = RateLimitMaxRequests;
				}

				this._rateLimitRemaining--;
				if (this._rateLimitRemaining < 0)
				{
					var delay = this._timestamp + 60 - now;
					this._logger.RateLimitExceeded(delay);
					await Task.Delay(TimeSpan.FromSeconds(Math.Min(delay, 60)), cancellationToken);
					this._timestamp = TimeProvider.System.GetUtcNow().ToUnixTimeSeconds();
					this._rateLimitRemaining = RateLimitMaxRequests;
				}

				response = await base.SendAsync(request, cancellationToken);

				if (response is { StatusCode: HttpStatusCode.TooManyRequests, Headers.RetryAfter.Delta: { } })
				{
					var delay = response.Headers.RetryAfter.Delta.Value.Add(TimeSpan.FromSeconds(1));
					this._logger.Got429HttpResponse(delay);
					await Task.Delay(delay, cancellationToken);
				}
				else
				{
					this._rateLimitRemaining = sbyte.Parse(response.Headers.GetValues("X-RateLimit-Remaining").First(), NumberFormatInfo.InvariantInfo);
					this._logger.RateLimitRemaining(this._rateLimitRemaining);
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