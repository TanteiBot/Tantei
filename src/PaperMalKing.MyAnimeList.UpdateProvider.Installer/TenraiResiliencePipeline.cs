// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Net;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.Timeout;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Installer;

internal static class TenraiResiliencePipeline
{
	private static readonly TimeSpan AttemptTimeout = TimeSpan.FromSeconds(5);
	private static readonly TimeSpan RetryDelay = TimeSpan.FromMilliseconds(500);

	public static void Configure(
		ResiliencePipelineBuilder<HttpResponseMessage> builder,
		TimeProvider timeProvider,
		TenraiRateLimiter rateLimiter)
	{
		ArgumentNullException.ThrowIfNull(builder);
		ArgumentNullException.ThrowIfNull(timeProvider);
		ArgumentNullException.ThrowIfNull(rateLimiter);

		builder.TimeProvider = timeProvider;
		builder.AddRetry(new HttpRetryStrategyOptions
		{
			BackoffType = DelayBackoffType.Constant,
			Delay = RetryDelay,
			MaxRetryAttempts = 1,
			ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
				.Handle<HttpRequestException>()
				.HandleResult(static response => IsRetryable(response.StatusCode)),
			ShouldRetryAfterHeader = true,
			UseJitter = true,
		});
		builder.AddRateLimiter(rateLimiter);
		builder.AddTimeout(new TimeoutStrategyOptions { Timeout = AttemptTimeout, });
	}

	public static ResiliencePipeline<HttpResponseMessage> Create(TimeProvider timeProvider, TenraiRateLimiter rateLimiter)
	{
		var builder = new ResiliencePipelineBuilder<HttpResponseMessage>();
		Configure(builder, timeProvider, rateLimiter);
		return builder.Build();
	}

	private static bool IsRetryable(HttpStatusCode statusCode) => statusCode is
		HttpStatusCode.RequestTimeout or
		HttpStatusCode.InternalServerError or
		HttpStatusCode.BadGateway or
		HttpStatusCode.ServiceUnavailable or
		HttpStatusCode.GatewayTimeout;
}
