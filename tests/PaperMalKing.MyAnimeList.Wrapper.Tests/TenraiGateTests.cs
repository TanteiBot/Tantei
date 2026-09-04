// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PaperMalKing.MyAnimeList.Wrapper.Tenrai;
using Polly.RateLimiting;
using Polly.Timeout;

namespace PaperMalKing.MyAnimeList.Wrapper.Tests;

public sealed class TenraiGateTests
{
	private const int CircuitOpenedEventId = 6;
	private const int CircuitClosedEventId = 7;
	private const int CooldownEngagedEventId = 11;
	private const int FailureThreshold = 5;
	private const int TwoEngagements = 2;
	private static readonly DateTimeOffset Start = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
	private static readonly TimeSpan OpenDuration = TimeSpan.FromSeconds(30);
	private static readonly TimeSpan Window = TimeSpan.FromSeconds(30);

	[Test]
	public async Task FourFailuresWithinTheWindowKeepCallsFlowing()
	{
		var gate = CreateGate(new FakeTimeProvider(Start));

		RecordFailures(gate, FailureThreshold - 1);

		await Assert.That(gate.Check()).IsNull();
	}

	[Test]
	public async Task FifthFailureWithinTheWindowSuppressesCallsWithCircuitOpen()
	{
		var gate = CreateGate(new FakeTimeProvider(Start));

		RecordFailures(gate, FailureThreshold);

		await Assert.That(gate.Check()).IsEqualTo(TenraiSuppression.CircuitOpen);
	}

	[Test]
	public async Task FailuresSpreadAcrossTheWindowStillAccumulate()
	{
		var time = new FakeTimeProvider(Start);
		var gate = CreateGate(time);
		RecordFailures(gate, FailureThreshold - 1);

		time.Advance(Window - TimeSpan.FromSeconds(1));
		await Assert.That(gate.Check()).IsNull();
		RecordFailures(gate, count: 1);

		await Assert.That(gate.Check()).IsEqualTo(TenraiSuppression.CircuitOpen);
	}

	[Test]
	public async Task FailuresOlderThanTheWindowAreEvicted()
	{
		var time = new FakeTimeProvider(Start);
		var gate = CreateGate(time);
		RecordFailures(gate, FailureThreshold - 1);

		time.Advance(Window + TimeSpan.FromSeconds(1));
		RecordFailures(gate, FailureThreshold - 1);

		await Assert.That(gate.Check()).IsNull();
		RecordFailures(gate, count: 1);
		await Assert.That(gate.Check()).IsEqualTo(TenraiSuppression.CircuitOpen);
	}

	[Test]
	public async Task OpenCircuitClosesAfterThirtySeconds()
	{
		var time = new FakeTimeProvider(Start);
		var gate = CreateGate(time);
		RecordFailures(gate, FailureThreshold);

		time.Advance(OpenDuration - TimeSpan.FromTicks(1));
		await Assert.That(gate.Check()).IsEqualTo(TenraiSuppression.CircuitOpen);
		time.Advance(TimeSpan.FromTicks(1));

		await Assert.That(gate.Check()).IsNull();
	}

	[Test]
	public async Task FailuresRecordedWhileOpenDoNotExtendTheOpenWindow()
	{
		var time = new FakeTimeProvider(Start);
		var gate = CreateGate(time);
		RecordFailures(gate, FailureThreshold);

		time.Advance(TimeSpan.FromSeconds(15));
		RecordFailures(gate, count: 1);
		time.Advance(TimeSpan.FromSeconds(15));

		await Assert.That(gate.Check()).IsNull();
	}

	[Test]
	public async Task CompletedResponsesTheClassificationCallsTransientOpenTheCircuit()
	{
		var gate = CreateGate(new FakeTimeProvider(Start));

		RecordCompleted(gate, HttpStatusCode.InternalServerError, FailureThreshold);

		await Assert.That(gate.Check()).IsEqualTo(TenraiSuppression.CircuitOpen);
	}

	[Test]
	public async Task CompletedResponsesTheClassificationExcludesNeverOpenTheCircuit()
	{
		var gate = CreateGate(new FakeTimeProvider(Start));

		RecordCompleted(gate, HttpStatusCode.BadRequest, FailureThreshold);

		await Assert.That(gate.Check()).IsNull();
	}

	[Test]
	public async Task SuccessfulOutcomesDoNotEraseLiveFailureTimestamps()
	{
		var gate = CreateGate(new FakeTimeProvider(Start));
		RecordFailures(gate, FailureThreshold - 1);

		RecordCompleted(gate, HttpStatusCode.OK, FailureThreshold);

		await Assert.That(gate.Check()).IsNull();
		RecordFailures(gate, count: 1);
		await Assert.That(gate.Check()).IsEqualTo(TenraiSuppression.CircuitOpen);
	}

	[Test]
	public async Task FailuresBelowThresholdEmitNoTransitionLog()
	{
		var logger = new RecordingLogger<TenraiGate>();
		var gate = new TenraiGate(new FakeTimeProvider(Start), logger);

		RecordFailures(gate, FailureThreshold - 1);

		await Assert.That(logger.Entries).IsEmpty();
	}

	[Test]
	public async Task OpeningEmitsExactlyOneWarning()
	{
		var logger = new RecordingLogger<TenraiGate>();
		var gate = new TenraiGate(new FakeTimeProvider(Start), logger);

		RecordFailures(gate, FailureThreshold);

		var opened = logger.Single();
		await Assert.That(opened.Level).IsEqualTo(LogLevel.Warning);
		await Assert.That(opened.EventId.Id).IsEqualTo(CircuitOpenedEventId);
	}

	[Test]
	public async Task ClosingAfterTheWindowEmitsExactlyOneClosedWarning()
	{
		var logger = new RecordingLogger<TenraiGate>();
		var time = new FakeTimeProvider(Start);
		var gate = new TenraiGate(time, logger);
		RecordFailures(gate, FailureThreshold);

		time.Advance(OpenDuration);
		_ = gate.Check();
		_ = gate.Check();

		await Assert.That(logger.Entries.Count(static entry => entry.EventId.Id == CircuitOpenedEventId)).IsEqualTo(1);
		var closed = logger.Entries.Single(static entry => entry.EventId.Id == CircuitClosedEventId);
		await Assert.That(closed.Level).IsEqualTo(LogLevel.Warning);
	}

	[Test]
	public async Task FailuresWhileOpenDoNotEmitAdditionalOpenWarnings()
	{
		var logger = new RecordingLogger<TenraiGate>();
		var time = new FakeTimeProvider(Start);
		var gate = new TenraiGate(time, logger);
		RecordFailures(gate, FailureThreshold);

		time.Advance(TimeSpan.FromSeconds(10));
		RecordFailures(gate, count: 1);

		await Assert.That(logger.Entries.Count(static entry => entry.EventId.Id == CircuitOpenedEventId)).IsEqualTo(1);
		await Assert.That(logger.Entries.Exists(static entry => entry.EventId.Id == CircuitClosedEventId)).IsFalse();
	}

	[Test]
	public async Task ActiveCooldownSuppressesNewOperations()
	{
		var time = new FakeTimeProvider(Start);
		var gate = CreateGate(time);
		using var response = RateLimited(HttpStatusCode.TooManyRequests, TimeSpan.FromSeconds(3));

		var retryAfter = gate.Record(TenraiSignal.Attempted(response));

		await Assert.That(retryAfter).IsEqualTo(TimeSpan.FromSeconds(3));
		await Assert.That(gate.Check()).IsEqualTo(TenraiSuppression.Cooldown);
		time.Advance(TimeSpan.FromSeconds(3));
		await Assert.That(gate.Check()).IsNull();
	}

	[Test]
	public async Task HttpDateRetryAfterEngagesTheCooldown()
	{
		var time = new FakeTimeProvider(Start);
		var gate = CreateGate(time);
		using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests)
		{
			Headers = { RetryAfter = new RetryConditionHeaderValue(Start + TimeSpan.FromSeconds(4)), },
		};

		var retryAfter = gate.Record(TenraiSignal.Attempted(response));

		await Assert.That(retryAfter).IsEqualTo(TimeSpan.FromSeconds(4));
		await Assert.That(gate.Check()).IsEqualTo(TenraiSuppression.Cooldown);
	}

	[Test]
	[Arguments(null)]
	[Arguments("invalid")]
	public async Task UnusableRetryAfterNeverEngagesTheCooldown(string? retryAfter)
	{
		var gate = CreateGate(new FakeTimeProvider(Start));
		using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);
		if (retryAfter is not null)
		{
			_ = response.Headers.TryAddWithoutValidation("Retry-After", retryAfter);
		}

		var recorded = gate.Record(TenraiSignal.Attempted(response));

		await Assert.That(recorded).IsNull();
		await Assert.That(gate.Check()).IsNull();
	}

	[Test]
	public async Task RetryAfterOnAStatusTheClassificationDoesNotGateIsIgnored()
	{
		var gate = CreateGate(new FakeTimeProvider(Start));
		using var response = RateLimited(HttpStatusCode.InternalServerError, TimeSpan.FromSeconds(30));

		var recorded = gate.Record(TenraiSignal.Attempted(response));

		await Assert.That(recorded).IsNull();
		await Assert.That(gate.Check()).IsNull();
	}

	[Test]
	public async Task LongerExpiryExtendsTheCooldownAndShorterOneDoesNot()
	{
		var time = new FakeTimeProvider(Start);
		var gate = CreateGate(time);
		using var longer = RateLimited(HttpStatusCode.TooManyRequests, TimeSpan.FromSeconds(10));
		using var shorter = RateLimited(HttpStatusCode.TooManyRequests, TimeSpan.FromSeconds(1));

		_ = gate.Record(TenraiSignal.Attempted(longer));
		_ = gate.Record(TenraiSignal.Attempted(shorter));

		time.Advance(TimeSpan.FromSeconds(9));
		await Assert.That(gate.Check()).IsEqualTo(TenraiSuppression.Cooldown);
		time.Advance(TimeSpan.FromSeconds(1));
		await Assert.That(gate.Check()).IsNull();
	}

	[Test]
	public async Task OverlongRetryAfterDoesNotOverflowTheCooldown()
	{
		var time = new FakeTimeProvider(Start);
		var gate = CreateGate(time);
		using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests)
		{
			Headers = { RetryAfter = new RetryConditionHeaderValue(DateTimeOffset.MaxValue), },
		};

		_ = gate.Record(TenraiSignal.Attempted(response));

		await Assert.That(gate.Check()).IsEqualTo(TenraiSuppression.Cooldown);
	}

	[Test]
	public async Task OpenCircuitTakesPrecedenceOverAnActiveCooldown()
	{
		var gate = CreateGate(new FakeTimeProvider(Start));
		using var response = RateLimited(HttpStatusCode.TooManyRequests, TimeSpan.FromSeconds(3));
		_ = gate.Record(TenraiSignal.Attempted(response));

		RecordFailures(gate, FailureThreshold);

		await Assert.That(gate.Check()).IsEqualTo(TenraiSuppression.CircuitOpen);
	}

	[Test]
	public async Task EngagingTheCooldownEmitsOneWarningWithTheDelay()
	{
		var logger = new RecordingLogger<TenraiGate>();
		var gate = new TenraiGate(new FakeTimeProvider(Start), logger);
		using var response = RateLimited(HttpStatusCode.TooManyRequests, TimeSpan.FromSeconds(3));

		_ = gate.Record(TenraiSignal.Attempted(response));

		var entry = logger.Single();
		await Assert.That(entry.Level).IsEqualTo(LogLevel.Warning);
		await Assert.That(entry.EventId.Id).IsEqualTo(CooldownEngagedEventId);
		await Assert.That(Field(entry, "RetryAfter")).IsEqualTo(TimeSpan.FromSeconds(3).ToString());
	}

	[Test]
	public async Task ExtendingAnActiveCooldownDoesNotEmitAnotherWarning()
	{
		var logger = new RecordingLogger<TenraiGate>();
		var time = new FakeTimeProvider(Start);
		var gate = new TenraiGate(time, logger);
		using var first = RateLimited(HttpStatusCode.TooManyRequests, TimeSpan.FromSeconds(5));
		using var second = RateLimited(HttpStatusCode.TooManyRequests, TimeSpan.FromSeconds(5));

		_ = gate.Record(TenraiSignal.Attempted(first));
		time.Advance(TimeSpan.FromSeconds(1));
		_ = gate.Record(TenraiSignal.Attempted(second));

		await Assert.That(logger.Entries.Count(static entry => entry.EventId.Id == CooldownEngagedEventId)).IsEqualTo(1);
	}

	[Test]
	public async Task ReEngagingAfterExpiryEmitsAnotherWarning()
	{
		var logger = new RecordingLogger<TenraiGate>();
		var time = new FakeTimeProvider(Start);
		var gate = new TenraiGate(time, logger);
		using var first = RateLimited(HttpStatusCode.TooManyRequests, TimeSpan.FromSeconds(2));
		using var second = RateLimited(HttpStatusCode.TooManyRequests, TimeSpan.FromSeconds(2));

		_ = gate.Record(TenraiSignal.Attempted(first));
		time.Advance(TimeSpan.FromSeconds(3));
		_ = gate.Record(TenraiSignal.Attempted(second));

		await Assert.That(logger.Entries.Count(static entry => entry.EventId.Id == CooldownEngagedEventId)).IsEqualTo(TwoEngagements);
	}

	[Test]
	public async Task MissingRetryAfterEmitsNoWarning()
	{
		var logger = new RecordingLogger<TenraiGate>();
		var gate = new TenraiGate(new FakeTimeProvider(Start), logger);
		using var response = new HttpResponseMessage(HttpStatusCode.TooManyRequests);

		_ = gate.Record(TenraiSignal.Attempted(response));

		await Assert.That(logger.Entries).IsEmpty();
	}

	[Test]
	public async Task HandlerRecordsNetworkFailuresAgainstTheCircuit()
	{
		using var scope = new HandlerScope((_, _) => Task.FromException<HttpResponseMessage>(new HttpRequestException("network")));

		await scope.SendManyAsync(FailureThreshold);

		await Assert.That(scope.Gate.Check()).IsEqualTo(TenraiSuppression.CircuitOpen);
	}

	[Test]
	public async Task HandlerRecordsInternalTimeoutsAgainstTheCircuit()
	{
		using var scope = new HandlerScope((_, _) => Task.FromException<HttpResponseMessage>(new TimeoutRejectedException("timeout")));

		await scope.SendManyAsync(FailureThreshold);

		await Assert.That(scope.Gate.Check()).IsEqualTo(TenraiSuppression.CircuitOpen);
	}

	[Test]
	public async Task HandlerNeverRecordsQueueRejectionAgainstTheCircuit()
	{
		using var scope = new HandlerScope((_, _) =>
			Task.FromException<HttpResponseMessage>(new RateLimiterRejectedException("suppressed")));

		await scope.SendManyAsync(FailureThreshold);

		await Assert.That(scope.Gate.Check()).IsNull();
	}

	[Test]
	public async Task HandlerNeverRecordsCallerCancellationAgainstTheCircuit()
	{
		using var cancellationSource = new CancellationTokenSource();
		cancellationSource.Cancel();
		using var scope = new HandlerScope((_, token) => Task.FromException<HttpResponseMessage>(new OperationCanceledException(token)));

		for (var attempt = 0; attempt < FailureThreshold; attempt++)
		{
			try
			{
				using var response = await scope.Client.GetAsync("anime/1", cancellationSource.Token);
			}
			catch (OperationCanceledException exception)
			{
				_ = exception;
			}
		}

		await Assert.That(scope.Gate.Check()).IsNull();
	}

	[Test]
	public async Task HandlerFailsFastWhileTheGateSuppresses()
	{
		var attempts = 0;
		using var scope = new HandlerScope((_, _) =>
		{
			_ = Interlocked.Increment(ref attempts);
			return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
		});
		using var rateLimited = RateLimited(HttpStatusCode.TooManyRequests, TimeSpan.FromSeconds(3));
		_ = scope.Gate.Record(TenraiSignal.Attempted(rateLimited));

		var suppressed = await Assert.That(async () =>
			await scope.Client.GetAsync("anime/1", TestContext.Current!.Execution.CancellationToken)).Throws<TenraiSuppressedException>();

		await Assert.That(suppressed!.Reason).IsEqualTo(TenraiSuppression.Cooldown);
		await Assert.That(attempts).IsEqualTo(0);
	}

	private static TenraiGate CreateGate(TimeProvider timeProvider) => new(timeProvider, NullLogger<TenraiGate>.Instance);

	private static void RecordFailures(TenraiGate gate, int count)
	{
		for (var failure = 0; failure < count; failure++)
		{
			_ = gate.Record(TenraiSignal.Failed);
		}
	}

	private static void RecordCompleted(TenraiGate gate, HttpStatusCode statusCode, int count)
	{
		for (var attempt = 0; attempt < count; attempt++)
		{
			using var response = new HttpResponseMessage(statusCode);
			_ = gate.Record(TenraiSignal.Completed(response));
		}
	}

	private static HttpResponseMessage RateLimited(HttpStatusCode statusCode, TimeSpan retryAfter)
	{
		var response = new HttpResponseMessage(statusCode);
		response.Headers.RetryAfter = new RetryConditionHeaderValue(retryAfter);
		return response;
	}

	private static string? Field(RecordedLogEntry entry, string name) =>
		entry.State.SingleOrDefault(field => string.Equals(field.Key, name, StringComparison.Ordinal)).Value?.ToString();

	private sealed class HandlerScope : IDisposable
	{
		private readonly TenraiGateHandler _handler;
		private readonly FakeHttpMessageHandler _inner;

		public HandlerScope(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond)
		{
			this.Gate = new(new FakeTimeProvider(Start), NullLogger<TenraiGate>.Instance);
			this._inner = new(respond);
			this._handler = new(this.Gate) { InnerHandler = this._inner, };
			this.Client = new(this._handler, disposeHandler: false)
			{
				BaseAddress = new("https://example.test/v1/"),
			};
		}

		public TenraiGate Gate { get; }

		public HttpClient Client { get; }

		public async Task SendManyAsync(int count)
		{
			for (var attempt = 0; attempt < count; attempt++)
			{
				try
				{
					using var response = await this.Client.GetAsync(
						"anime/1", TestContext.Current!.Execution.CancellationToken);
				}
				catch (Exception exception) when (exception is not OperationCanceledException)
				{
					_ = exception;
				}
			}
		}

		public void Dispose()
		{
			this.Client.Dispose();
			this._handler.Dispose();
			this._inner.Dispose();
		}
	}
}
