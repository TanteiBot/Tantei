// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PaperMalKing.AniList.Wrapper;

internal sealed class HeaderBasedRateLimitMessageHandler : DelegatingHandler
{
	private readonly ILogger<HeaderBasedRateLimitMessageHandler> _logger;
	private const sbyte RateLimitMaxRequests = 90;
	private sbyte _rateLimitRemaining;
	private long _timestamp;
	private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

	public HeaderBasedRateLimitMessageHandler(ILogger<HeaderBasedRateLimitMessageHandler> logger)
	{
		this._logger = logger;
		this._rateLimitRemaining = RateLimitMaxRequests;
		this._timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
	}

	protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
	{
		await this._semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);
		try
		{
			HttpResponseMessage response;
			do
			{
				var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
				if (now - this._timestamp >= 60)
				{
					this._logger.LogTrace("Resetting AniList ratelimiter");
					this._timestamp = now;
					this._rateLimitRemaining = RateLimitMaxRequests;
				}

				this._rateLimitRemaining--;
				if (this._rateLimitRemaining < 0)
				{
					var delay = this._timestamp + 60 - now;
					this._logger.LogDebug("AniList exceeded rate-limit waiting {Delay}", delay);
					await Task.Delay(TimeSpan.FromSeconds(Math.Min(delay, 60)), cancellationToken).ConfigureAwait(false);
					this._timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
					this._rateLimitRemaining = RateLimitMaxRequests;
				}

				response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

				if (response is { StatusCode: HttpStatusCode.TooManyRequests, Headers.RetryAfter.Delta: { } })
				{
					var delay = response.Headers.RetryAfter.Delta.Value.Add(TimeSpan.FromSeconds(1));
					this._logger.LogInformation("Got 429'd waiting {Delay}", delay);
					await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
				}
				else
				{
					this._rateLimitRemaining = sbyte.Parse(response.Headers.GetValues("X-RateLimit-Remaining").First());
					this._logger.LogTrace("AniList rate limit remaining {RateLimitRemaining}", this._rateLimitRemaining);
				}
			} while (!cancellationToken.IsCancellationRequested && response.StatusCode == HttpStatusCode.TooManyRequests);

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