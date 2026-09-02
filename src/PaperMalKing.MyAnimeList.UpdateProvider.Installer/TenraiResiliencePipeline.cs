// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Net;
using Microsoft.Extensions.Http.Resilience;
using PaperMalKing.MyAnimeList.Wrapper.Tenrai;
using Polly;
using Polly.Retry;
using Polly.Timeout;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Installer;

internal static class TenraiResiliencePipeline
{
	private static readonly TimeSpan AttemptTimeout = TimeSpan.FromSeconds(5);
	private static readonly TimeSpan MaximumRetryAfterDelay = TimeSpan.FromSeconds(5);
	private static readonly TimeSpan MinimumRetryAfterDelay = TimeSpan.FromTicks(1L);
	private static readonly TimeSpan RetryDelay = TimeSpan.FromMilliseconds(500);

	public static void Configure(
		ResiliencePipelineBuilder<HttpResponseMessage> builder,
		TimeProvider timeProvider,
		TenraiRateLimiter rateLimiter,
		TenraiCooldown cooldown,
		TenraiEnrichmentTelemetry telemetry)
	{
		ArgumentNullException.ThrowIfNull(builder);
		ArgumentNullException.ThrowIfNull(timeProvider);
		ArgumentNullException.ThrowIfNull(rateLimiter);
		ArgumentNullException.ThrowIfNull(cooldown);
		ArgumentNullException.ThrowIfNull(telemetry);

		builder.TimeProvider = timeProvider;
		builder.AddRetry(new HttpRetryStrategyOptions
		{
			BackoffType = DelayBackoffType.Constant,
			Delay = RetryDelay,
			MaxRetryAttempts = 1,
			DelayGenerator = arguments => RetryDelayAsync(arguments, cooldown),
			ShouldHandle = arguments => ShouldRetryAsync(arguments, cooldown, telemetry),
			OnRetry = _ =>
			{
				telemetry.Current?.RecordRetry();
				return default;
			},
			ShouldRetryAfterHeader = false,
			UseJitter = true,
		});
		builder.AddRateLimiter(rateLimiter);
		builder.AddTimeout(new TimeoutStrategyOptions { Timeout = AttemptTimeout, });
	}

	public static ResiliencePipeline<HttpResponseMessage> Create(
		TimeProvider timeProvider,
		TenraiRateLimiter rateLimiter,
		TenraiCooldown cooldown,
		TenraiEnrichmentTelemetry telemetry)
	{
		var builder = new ResiliencePipelineBuilder<HttpResponseMessage>();
		Configure(builder, timeProvider, rateLimiter, cooldown, telemetry);
		return builder.Build();
	}

	private static ValueTask<TimeSpan?> RetryDelayAsync(
		RetryDelayGeneratorArguments<HttpResponseMessage> arguments,
		TenraiCooldown cooldown) => ValueTask.FromResult(arguments.Outcome.Result is { } response
		? MinimumPositive(cooldown.GetRetryAfter(response))
		: null);

	private static TimeSpan? MinimumPositive(TimeSpan? delay) => delay == TimeSpan.Zero ? MinimumRetryAfterDelay : delay;

	private static ValueTask<bool> ShouldRetryAsync(
		RetryPredicateArguments<HttpResponseMessage> arguments,
		TenraiCooldown cooldown,
		TenraiEnrichmentTelemetry telemetry)
	{
		if (arguments.Outcome.Exception is HttpRequestException)
		{
			return ValueTask.FromResult(true);
		}

		if (arguments.Outcome.Result is not { } response)
		{
			return ValueTask.FromResult(false);
		}

		var retryAfter = cooldown.ApplyRetryAfter(response);
		telemetry.Current?.RecordRetryAfter(retryAfter);
		var shouldRetry = response.StatusCode switch
		{
			HttpStatusCode.TooManyRequests => retryAfter <= MaximumRetryAfterDelay,
			HttpStatusCode.ServiceUnavailable when retryAfter is not null => retryAfter <= MaximumRetryAfterDelay,
			_ => IsRetryable(response.StatusCode),
		};
		return ValueTask.FromResult(shouldRetry);
	}

	private static bool IsRetryable(HttpStatusCode statusCode) => statusCode is
		HttpStatusCode.RequestTimeout or
		HttpStatusCode.InternalServerError or
		HttpStatusCode.BadGateway or
		HttpStatusCode.ServiceUnavailable or
		HttpStatusCode.GatewayTimeout;
}
