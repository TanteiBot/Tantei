// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Http.Resilience;
using PaperMalKing.Common.RateLimiters;
using Polly;
using Polly.Retry;
using Polly.Timeout;

namespace PaperMalKing.MyAnimeList.Wrapper.Tenrai;

internal static class TenraiResiliencePipeline
{
	private const int QueueLimit = 10;
	private const int TokenLimit = 2;
	private const int TokensPerPeriod = 1;
	private static readonly TimeSpan AttemptTimeout = TimeSpan.FromSeconds(5);
	private static readonly TimeSpan MaximumRetryAfterDelay = TimeSpan.FromSeconds(5);
	private static readonly TimeSpan MinimumRetryAfterDelay = TimeSpan.FromTicks(1L);
	private static readonly TimeSpan ReplenishmentPeriod = TimeSpan.FromSeconds(2.4D);
	private static readonly TimeSpan RetryDelay = TimeSpan.FromMilliseconds(500);

	[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Its candled in created ratelimiter")]
	public static RateLimiter<TenraiClient> CreateRateLimiter() => new(new TokenBucketRateLimiter(new()
	{
		AutoReplenishment = true,
		QueueLimit = QueueLimit,
		QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
		ReplenishmentPeriod = ReplenishmentPeriod,
		TokenLimit = TokenLimit,
		TokensPerPeriod = TokensPerPeriod,
	}));

	public static void Configure(
		ResiliencePipelineBuilder<HttpResponseMessage> builder,
		TimeProvider timeProvider,
		RateLimiter rateLimiter,
		TenraiCooldown cooldown)
	{
		ArgumentNullException.ThrowIfNull(builder);
		ArgumentNullException.ThrowIfNull(timeProvider);
		ArgumentNullException.ThrowIfNull(rateLimiter);
		ArgumentNullException.ThrowIfNull(cooldown);

		builder.TimeProvider = timeProvider;
		builder.AddRetry(new HttpRetryStrategyOptions
		{
			BackoffType = DelayBackoffType.Constant,
			Delay = RetryDelay,
			MaxRetryAttempts = 1,
			DelayGenerator = arguments => RetryDelayAsync(arguments, cooldown),
			ShouldHandle = arguments => ShouldRetryAsync(arguments, cooldown),
			OnRetry = arguments =>
			{
				AttemptFor(arguments.Context)?.RecordRetry();
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
		RateLimiter rateLimiter,
		TenraiCooldown cooldown)
	{
		var builder = new ResiliencePipelineBuilder<HttpResponseMessage>();
		Configure(builder, timeProvider, rateLimiter, cooldown);
		return builder.Build();
	}

	private static ValueTask<TimeSpan?> RetryDelayAsync(
		RetryDelayGeneratorArguments<HttpResponseMessage> arguments,
		TenraiCooldown cooldown) => ValueTask.FromResult(arguments.Outcome.Result is { } response
		? MinimumPositive(cooldown.GetRetryAfter(response))
		: null);

	private static TimeSpan? MinimumPositive(TimeSpan? delay) => delay == TimeSpan.Zero ? MinimumRetryAfterDelay : delay;

	private static TenraiAttempt? AttemptFor(ResilienceContext context) => TenraiAttempt.From(context.GetRequestMessage());

	private static ValueTask<bool> ShouldRetryAsync(
		RetryPredicateArguments<HttpResponseMessage> arguments,
		TenraiCooldown cooldown)
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
		AttemptFor(arguments.Context)?.RecordRetryAfter(retryAfter);
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
