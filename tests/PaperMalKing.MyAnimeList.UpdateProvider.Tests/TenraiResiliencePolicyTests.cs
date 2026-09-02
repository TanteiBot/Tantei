// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging.Abstractions;
using PaperMalKing.MyAnimeList.UpdateProvider.Installer;
using PaperMalKing.MyAnimeList.UpdateProvider.Tests.Search;
using PaperMalKing.MyAnimeList.Wrapper;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models;
using PaperMalKing.MyAnimeList.Wrapper.Tenrai;
using Polly.RateLimiting;
using Polly.Timeout;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Tests;

[SuppressMessage("Usage", "VSTHRD003:Avoid awaiting foreign Tasks", Justification = "The tasks are started by each test")]
public sealed class TenraiResiliencePolicyTests
{
	private const string AnimePath = "anime/1";
	private const int AcceptedBeforeQueueRejection = 12;
	private const int InitialBurst = 2;
	private const int NoRetryAttemptCount = 1;
	private const int QueueLimit = 10;
	private const int RequestsInFirstMinute = 27;
	private const int RetryAttemptCount = 2;
	private static readonly DateTimeOffset Start = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
	private static readonly TimeSpan ReplenishmentPeriod = TimeSpan.FromSeconds(2.4D);

	[Test]
	public async Task LimiterAllowsTwoImmediatelyThenSustainsTwentyFiveRequestsPerMinute()
	{
		var attempts = 0;
		using var scope = new PolicyScope((_, _) =>
		{
			Interlocked.Increment(ref attempts);
			return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
		});

		for (var request = 0; request < RequestsInFirstMinute; request++)
		{
			var path = "anime/" + request.ToString(CultureInfo.InvariantCulture);
			var responseTask = scope.Client.GetAsync(path, TestContext.Current!.Execution.CancellationToken);
			if (request >= InitialBurst)
			{
				await Assert.That(responseTask.IsCompleted).IsFalse();
				scope.Time.Advance(ReplenishmentPeriod);
			}

			using var response = await responseTask;
		}

		var queued = scope.Client.GetAsync("anime/27", TestContext.Current!.Execution.CancellationToken);
		await Assert.That(attempts).IsEqualTo(RequestsInFirstMinute);
		await Assert.That(queued.IsCompleted).IsFalse();
	}

	[Test]
	public async Task LimiterQueuesTenOldestFirstAndRejectsTheEleventhQueuedRequest()
	{
		var started = new ConcurrentQueue<int>();
		using var scope = new PolicyScope((request, _) =>
		{
			started.Enqueue(int.Parse(request.RequestUri!.Segments[^1], CultureInfo.InvariantCulture));
			return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
		});
		var requests = Enumerable.Range(0, AcceptedBeforeQueueRejection + 1)
			.Select(index => scope.Client.GetAsync(
				"anime/" + index.ToString(CultureInfo.InvariantCulture),
				TestContext.Current!.Execution.CancellationToken))
			.ToArray();

		await Assert.That(async () => await requests[^1]).Throws<RateLimiterRejectedException>();
		for (var index = InitialBurst; index < AcceptedBeforeQueueRejection; index++)
		{
			scope.Time.Advance(ReplenishmentPeriod);
			using var response = await requests[index];
		}

		var responses = await Task.WhenAll(requests.Take(AcceptedBeforeQueueRejection));
		foreach (var response in responses)
		{
			response.Dispose();
		}

		await Assert.That(string.Join(',', started)).IsEqualTo("0,1,2,3,4,5,6,7,8,9,10,11");
	}

	[Test]
	public async Task QueuedRequestHasNoInternalTimeoutAndCallerCancellationPropagates()
	{
		using var scope = new PolicyScope((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));
		var cancellationToken = TestContext.Current!.Execution.CancellationToken;
		using var first = await scope.Client.GetAsync(AnimePath, cancellationToken);
		using var second = await scope.Client.GetAsync("anime/2", cancellationToken);
		using var cancellationSource = new CancellationTokenSource();
		var queued = Enumerable.Range(0, QueueLimit)
			.Select(index => scope.Client.GetAsync(
				"queued/" + index.ToString(CultureInfo.InvariantCulture), cancellationSource.Token))
			.ToArray();

		scope.Time.Advance(TimeSpan.FromSeconds(5));
		await Assert.That(queued[^1].IsCompleted).IsFalse();
		cancellationSource.Cancel();

		var exception = await Assert.That(async () => await Task.WhenAll(queued)).Throws<OperationCanceledException>();
		var actual = exception ?? throw new InvalidOperationException();
		await Assert.That(actual.CancellationToken).IsEqualTo(cancellationSource.Token);
	}

	[Test]
	[Arguments(HttpStatusCode.RequestTimeout, RetryAttemptCount)]
	[Arguments(HttpStatusCode.BadRequest, NoRetryAttemptCount)]
	[Arguments(HttpStatusCode.TooManyRequests, NoRetryAttemptCount)]
	[Arguments(HttpStatusCode.InternalServerError, RetryAttemptCount)]
	[Arguments(HttpStatusCode.NotImplemented, NoRetryAttemptCount)]
	[Arguments(HttpStatusCode.BadGateway, RetryAttemptCount)]
	[Arguments(HttpStatusCode.ServiceUnavailable, RetryAttemptCount)]
	[Arguments(HttpStatusCode.GatewayTimeout, RetryAttemptCount)]
	public async Task RetryStatusAllowlistIsNarrow(HttpStatusCode firstStatus, int expectedAttempts)
	{
		var attempts = 0;
		using var scope = new PolicyScope((_, _) => Task.FromResult(new HttpResponseMessage(
			Interlocked.Increment(ref attempts) is NoRetryAttemptCount ? firstStatus : HttpStatusCode.OK)));

		var responseTask = scope.Client.GetAsync(AnimePath, TestContext.Current!.Execution.CancellationToken);
		if (expectedAttempts is RetryAttemptCount)
		{
			scope.Time.Advance(TimeSpan.FromSeconds(2));
		}

		using var response = await responseTask;
		await Assert.That(attempts).IsEqualTo(expectedAttempts);
	}

	[Test]
	[Arguments(HttpStatusCode.TooManyRequests, false)]
	[Arguments(HttpStatusCode.TooManyRequests, true)]
	[Arguments(HttpStatusCode.ServiceUnavailable, false)]
	[Arguments(HttpStatusCode.ServiceUnavailable, true)]
	public async Task ValidShortRetryAfterRetriesCurrentOperationOnce(HttpStatusCode statusCode, bool useDate)
	{
		var attempts = 0;
		using var scope = new PolicyScope((_, _) =>
		{
			if (Interlocked.Increment(ref attempts) is RetryAttemptCount)
			{
				return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
			}

			var retryAfter = useDate
				? new RetryConditionHeaderValue(Start + TimeSpan.FromSeconds(5))
				: new RetryConditionHeaderValue(TimeSpan.FromSeconds(5));
			return Task.FromResult(CreateResponse(statusCode, retryAfter));
		});

		var responseTask = scope.Client.GetAsync(AnimePath, TestContext.Current!.Execution.CancellationToken);
		await Assert.That(responseTask.IsCompleted).IsFalse();
		scope.Time.Advance(TimeSpan.FromSeconds(5));

		using var response = await responseTask;
		await Assert.That(attempts).IsEqualTo(RetryAttemptCount);
	}

	[Test]
	[Arguments(HttpStatusCode.TooManyRequests)]
	[Arguments(HttpStatusCode.ServiceUnavailable)]
	public async Task ZeroRetryAfterRetriesCurrentOperationOnce(HttpStatusCode statusCode)
	{
		var attempts = 0;
		using var scope = new PolicyScope((_, _) => Task.FromResult(
			Interlocked.Increment(ref attempts) is NoRetryAttemptCount
				? CreateResponse(statusCode, new RetryConditionHeaderValue(TimeSpan.Zero))
				: new HttpResponseMessage(HttpStatusCode.OK)));

		var responseTask = scope.Client.GetAsync(AnimePath, TestContext.Current!.Execution.CancellationToken);
		await DrainContinuationsAsync();
		scope.Time.Advance(TimeSpan.FromSeconds(2));

		using var response = await responseTask;
		await Assert.That(attempts).IsEqualTo(RetryAttemptCount);
	}

	[Test]
	[Arguments(HttpStatusCode.TooManyRequests, null, NoRetryAttemptCount)]
	[Arguments(HttpStatusCode.TooManyRequests, "invalid", NoRetryAttemptCount)]
	[Arguments(HttpStatusCode.ServiceUnavailable, null, RetryAttemptCount)]
	[Arguments(HttpStatusCode.ServiceUnavailable, "invalid", RetryAttemptCount)]
	public async Task UnusableRetryAfterNeverCausesImmediateRetry(
		HttpStatusCode statusCode,
		string? retryAfter,
		int expectedAttempts)
	{
		var attempts = 0;
		using var scope = new PolicyScope((_, _) =>
		{
			var attemptStatus = Interlocked.Increment(ref attempts) is NoRetryAttemptCount ? statusCode : HttpStatusCode.OK;
			return Task.FromResult(CreateResponse(attemptStatus, retryAfter));
		});

		var responseTask = scope.Client.GetAsync(AnimePath, TestContext.Current!.Execution.CancellationToken);
		await Assert.That(attempts).IsEqualTo(NoRetryAttemptCount);
		if (expectedAttempts is RetryAttemptCount)
		{
			await Assert.That(responseTask.IsCompleted).IsFalse();
			scope.Time.Advance(TimeSpan.FromSeconds(2));
		}

		using var response = await responseTask;
		await Assert.That(attempts).IsEqualTo(expectedAttempts);
	}

	[Test]
	[Arguments(HttpStatusCode.TooManyRequests)]
	[Arguments(HttpStatusCode.ServiceUnavailable)]
	public async Task LongRetryAfterSuppressesAllEnrichmentOperations(HttpStatusCode statusCode)
	{
		var attempts = 0;
		using var scope = new PolicyScope((_, _) =>
		{
			Interlocked.Increment(ref attempts);
			return Task.FromResult(CreateResponse(statusCode, new RetryConditionHeaderValue(TimeSpan.FromSeconds(6))));
		});
		var client = CreateClient(scope);
		var cancellationToken = TestContext.Current!.Execution.CancellationToken;

		var initial = await client.GetAnimeDetailsAsync(1L, cancellationToken);
		var anime = await client.GetAnimeDetailsAsync(2L, cancellationToken);
		var manga = await client.GetMangaDetailsAsync(3L, cancellationToken);
		var seiyu = await client.GetAnimeSeiyuAsync(4L, cancellationToken);

		await Assert.That(initial).IsEqualTo(MediaInfo.Empty);
		await Assert.That(anime).IsEqualTo(MediaInfo.Empty);
		await Assert.That(manga).IsEqualTo(MediaInfo.Empty);
		await Assert.That(seiyu).IsEmpty();
		await Assert.That(attempts).IsEqualTo(NoRetryAttemptCount);

		scope.Time.Advance(TimeSpan.FromSeconds(6));
		_ = await client.GetMangaDetailsAsync(1L, cancellationToken);
		await Assert.That(attempts).IsEqualTo(RetryAttemptCount);
	}

	[Test]
	public async Task NetworkFailureRetriesOnlyOnce()
	{
		var attempts = 0;
		using var scope = new PolicyScope((_, _) =>
		{
			Interlocked.Increment(ref attempts);
			return Task.FromException<HttpResponseMessage>(new HttpRequestException("unavailable"));
		});
		var responseTask = scope.Client.GetAsync(AnimePath, TestContext.Current!.Execution.CancellationToken);

		scope.Time.Advance(TimeSpan.FromSeconds(2));

		await Assert.That(async () => await responseTask).Throws<HttpRequestException>();
		await Assert.That(attempts).IsEqualTo(RetryAttemptCount);
	}

	[Test]
	public async Task ResponseBodyNetworkFailureRetriesOnlyOnce()
	{
		var attempts = 0;
		using var scope = new PolicyScope((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
		{
			Content = Interlocked.Increment(ref attempts) is NoRetryAttemptCount
				? new FailingContent()
				: new StringContent("{}"),
		}));
		var responseTask = scope.Client.GetAsync(AnimePath, TestContext.Current!.Execution.CancellationToken);

		scope.Time.Advance(TimeSpan.FromSeconds(2));

		using var response = await responseTask;
		await Assert.That(attempts).IsEqualTo(RetryAttemptCount);
	}

	[Test]
	public async Task RetryReacquiresTheSharedLimiterPermit()
	{
		var attempts = 0;
		using var scope = new PolicyScope((request, _) =>
		{
			if (request.RequestUri?.AbsolutePath.EndsWith("warmup", StringComparison.Ordinal) is true)
			{
				return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
			}

			return Task.FromResult(new HttpResponseMessage(
				Interlocked.Increment(ref attempts) is NoRetryAttemptCount ? HttpStatusCode.ServiceUnavailable : HttpStatusCode.OK));
		});
		var cancellationToken = TestContext.Current!.Execution.CancellationToken;
		using var warmup = await scope.Client.GetAsync("anime/warmup", cancellationToken);
		var retried = scope.Client.GetAsync("anime/retry", cancellationToken);

		scope.Time.Advance(TimeSpan.FromSeconds(2));
		await DrainContinuationsAsync();
		await Assert.That(attempts).IsEqualTo(NoRetryAttemptCount);
		scope.Time.Advance(TimeSpan.FromMilliseconds(400));

		using var response = await retried;
		await Assert.That(attempts).IsEqualTo(RetryAttemptCount);
	}

	[Test]
	public async Task AttemptTimesOutAtFiveSecondsWithoutTimingOutLimiterQueue()
	{
		using var scope = new PolicyScope(async (_, cancellationToken) =>
		{
			await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
			return new HttpResponseMessage(HttpStatusCode.OK);
		});
		var responseTask = scope.Client.GetAsync(AnimePath, TestContext.Current!.Execution.CancellationToken);

		scope.Time.Advance(TimeSpan.FromMilliseconds(4_999));
		await Assert.That(responseTask.IsCompleted).IsFalse();
		scope.Time.Advance(TimeSpan.FromMilliseconds(1));

		await Assert.That(async () => await responseTask).Throws<TimeoutRejectedException>();
	}

	[Test]
	public async Task ResponseBodyTransportTimesOutAtFiveSeconds()
	{
		using var scope = new PolicyScope((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
		{
			Content = new SlowContent(),
		}));
		var responseTask = scope.Client.GetAsync(AnimePath, TestContext.Current!.Execution.CancellationToken);

		scope.Time.Advance(TimeSpan.FromMilliseconds(4_999));
		await Assert.That(responseTask.IsCompleted).IsFalse();
		scope.Time.Advance(TimeSpan.FromMilliseconds(1));

		await Assert.That(async () => await responseTask).Throws<TimeoutRejectedException>();
	}

	[Test]
	public async Task InternalAttemptTimeoutReturnsEmptyOnlyForRequestedEnrichment()
	{
		using var scope = new PolicyScope(async (_, cancellationToken) =>
		{
			await Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);
			return new HttpResponseMessage(HttpStatusCode.OK);
		});
		var client = CreateClient(scope);
		var resultTask = client.GetAnimeDetailsAsync(1L, TestContext.Current!.Execution.CancellationToken);

		scope.Time.Advance(TimeSpan.FromSeconds(5));

		var result = await resultTask;
		await Assert.That(result.Themes).IsEmpty();
		await Assert.That(result.Demographic).IsEmpty();
	}

	[Test]
	public async Task QueueRejectionReturnsEmptyEnrichment()
	{
		using var scope = new PolicyScope((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
		{
			Content = new StringContent("{\"data\":{}}"),
		}));
		var cancellationToken = TestContext.Current!.Execution.CancellationToken;
		using var first = await scope.Client.GetAsync("warmup/1", cancellationToken);
		using var second = await scope.Client.GetAsync("warmup/2", cancellationToken);
		using var queueCancellation = new CancellationTokenSource();
		var queued = Enumerable.Range(0, QueueLimit)
			.Select(index => scope.Client.GetAsync(
				"queued/" + index.ToString(CultureInfo.InvariantCulture), queueCancellation.Token))
			.ToArray();
		var client = CreateClient(scope);

		var result = await client.GetMangaDetailsAsync(1L, cancellationToken);

		await Assert.That(result.Themes).IsEmpty();
		await Assert.That(result.Demographic).IsEmpty();
		queueCancellation.Cancel();
		await Assert.That(async () => await Task.WhenAll(queued)).Throws<OperationCanceledException>();
	}

	[Test]
	public async Task ExhaustedTransientFailureThroughThePipelineOpensTheSharedCircuit()
	{
		const int failureThreshold = 5;
		using var scope = new PolicyScope((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)));
		for (var failure = 0; failure < failureThreshold - 1; failure++)
		{
			scope.Circuit.RecordTerminalFailure();
		}

		var client = CreateClient(scope);
		var resultTask = client.GetAnimeDetailsAsync(1L, TestContext.Current!.Execution.CancellationToken);
		scope.Time.Advance(TimeSpan.FromSeconds(1));

		var result = await resultTask;
		await Assert.That(result).IsEqualTo(MediaInfo.Empty);
		await Assert.That(scope.Circuit.IsOpen).IsTrue();
	}

	[Test]
	public async Task OpenCircuitFailsFastThroughTheFullPipeline()
	{
		const int failureThreshold = 5;
		var attempts = 0;
		using var scope = new PolicyScope((_, _) =>
		{
			Interlocked.Increment(ref attempts);
			return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"data\":{}}"), });
		});
		for (var failure = 0; failure < failureThreshold; failure++)
		{
			scope.Circuit.RecordTerminalFailure();
		}

		var client = CreateClient(scope);

		var result = await client.GetAnimeDetailsAsync(1L, TestContext.Current!.Execution.CancellationToken);

		await Assert.That(result).IsEqualTo(MediaInfo.Empty);
		await Assert.That(attempts).IsEqualTo(0);
	}

	private static MyAnimeListClient CreateClient(PolicyScope scope) =>
		new(NullLogger<MyAnimeListClient>.Instance, null!, null!, scope.Client, scope.Circuit);

	private static HttpResponseMessage CreateResponse(HttpStatusCode statusCode, RetryConditionHeaderValue retryAfter)
	{
		var response = new HttpResponseMessage(statusCode);
		response.Headers.RetryAfter = retryAfter;
		return response;
	}

	private static HttpResponseMessage CreateResponse(HttpStatusCode statusCode, string? retryAfter)
	{
		var response = new HttpResponseMessage(statusCode);
		if (retryAfter is not null)
		{
			_ = response.Headers.TryAddWithoutValidation("Retry-After", retryAfter);
		}

		return response;
	}

	private static async Task DrainContinuationsAsync()
	{
		const int continuationPasses = 10;
		for (var index = 0; index < continuationPasses; index++)
		{
			await Task.Yield();
		}
	}

	private sealed class PolicyScope : IDisposable
	{
		private readonly TenraiCircuitHandler _circuitHandler;
		private readonly TenraiCooldownHandler _cooldownHandler;
		private readonly TenraiRateLimiter _limiter;
		private readonly FakeHttpMessageHandler _primaryHandler;
		private readonly ResilienceHandler _resilienceHandler;

		public PolicyScope(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond)
		{
			this.Time = new(Start);
			var cooldown = new TenraiCooldown(this.Time);
			this.Circuit = new(this.Time);
			this._limiter = new(this.Time);
			this._primaryHandler = new(respond);
			this._resilienceHandler = new(TenraiResiliencePipeline.Create(this.Time, this._limiter, cooldown))
			{
				InnerHandler = new TenraiResponseBufferingHandler(this._primaryHandler),
			};
			this._cooldownHandler = new(cooldown) { InnerHandler = this._resilienceHandler, };
			this._circuitHandler = new(this.Circuit) { InnerHandler = this._cooldownHandler, };
			this.Client = new(this._circuitHandler, disposeHandler: false)
			{
				BaseAddress = new("https://example.test/v1/"),
				Timeout = Timeout.InfiniteTimeSpan,
			};
		}

		public TenraiCircuit Circuit { get; }

		public HttpClient Client { get; }

		public ManualTimeProvider Time { get; }

		public void Dispose()
		{
			this.Client.Dispose();
			this._circuitHandler.Dispose();
			this._cooldownHandler.Dispose();
			this._resilienceHandler.Dispose();
			this._primaryHandler.Dispose();
			this._limiter.Dispose();
		}
	}

	private sealed class FailingContent : HttpContent
	{
		protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
			Task.FromException(new HttpRequestException("response body failed"));

		protected override bool TryComputeLength(out long length)
		{
			length = 0L;
			return false;
		}
	}

	private sealed class FakeHttpMessageHandler(
		Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond) : HttpMessageHandler
	{
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
			respond(request, cancellationToken);
	}

	private sealed class SlowContent : HttpContent
	{
		protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
			Task.Delay(Timeout.InfiniteTimeSpan);

		protected override Task SerializeToStreamAsync(
			Stream stream,
			TransportContext? context,
			CancellationToken cancellationToken) => Task.Delay(Timeout.InfiniteTimeSpan, cancellationToken);

		protected override bool TryComputeLength(out long length)
		{
			length = 0L;
			return false;
		}
	}
}
