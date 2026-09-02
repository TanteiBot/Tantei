// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
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
	private static readonly TimeSpan MinimumRetryAfterDelay = TimeSpan.FromTicks(1L);
	private static readonly TimeSpan ReplenishmentPeriod = TimeSpan.FromSeconds(2.4D);
	private static readonly TimeSpan RetryDelay = TimeSpan.FromMilliseconds(500);
	private static readonly ResiliencePropertyKey<TimeSpan?> RetryAfterKey = new("Tenrai.RetryAfter");

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
		TenraiGate gate)
	{
		ArgumentNullException.ThrowIfNull(builder);
		ArgumentNullException.ThrowIfNull(timeProvider);
		ArgumentNullException.ThrowIfNull(rateLimiter);
		ArgumentNullException.ThrowIfNull(gate);

		builder.TimeProvider = timeProvider;
		builder.AddRetry(new HttpRetryStrategyOptions
		{
			BackoffType = DelayBackoffType.Constant,
			Delay = RetryDelay,
			MaxRetryAttempts = 1,
			DelayGenerator = RetryDelayAsync,
			ShouldHandle = arguments => ShouldRetryAsync(arguments, gate),
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

	private static ValueTask<TimeSpan?> RetryDelayAsync(RetryDelayGeneratorArguments<HttpResponseMessage> arguments) =>
		ValueTask.FromResult(arguments.Context.Properties.TryGetValue(RetryAfterKey, out var retryAfter)
			? MinimumPositive(retryAfter)
			: null);

	private static TimeSpan? MinimumPositive(TimeSpan? delay) => delay == TimeSpan.Zero ? MinimumRetryAfterDelay : delay;

	private static TenraiAttempt? AttemptFor(ResilienceContext context) => TenraiAttempt.From(context.GetRequestMessage());

	private static ValueTask<bool> ShouldRetryAsync(
		RetryPredicateArguments<HttpResponseMessage> arguments,
		TenraiGate gate)
	{
		if (arguments.Outcome.Result is not { } response)
		{
			arguments.Context.Properties.Set(RetryAfterKey, value: null);
			return ValueTask.FromResult(arguments.Outcome.Exception is HttpRequestException);
		}

		var retryAfter = gate.Record(TenraiSignal.Attempted(response));
		arguments.Context.Properties.Set(RetryAfterKey, retryAfter);
		AttemptFor(arguments.Context)?.RecordRetryAfter(retryAfter);
		return ValueTask.FromResult(TenraiClassification.ShouldRetry(TenraiClassification.Classify(response.StatusCode), retryAfter));
	}
}
