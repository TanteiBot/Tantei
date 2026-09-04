// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models;
using PaperMalKing.MyAnimeList.Wrapper.Tenrai;

namespace PaperMalKing.MyAnimeList.Wrapper.Tests;

[SuppressMessage("Usage", "VSTHRD003:Avoid awaiting foreign Tasks", Justification = "The tasks are started by each test")]
public sealed class TenraiEnrichmentLoggingThroughPipelineTests
{
	private const int CooldownSuppressedEventId = 4;
	private const int QueueRejectedEventId = 5;
	private const int QueueLimit = 10;
	private const int TerminalFailureEventId = 1;
	private const long SecondMediaId = 2L;
	private static readonly TimeSpan CooldownRetryAfter = TimeSpan.FromSeconds(6);
	private static readonly DateTimeOffset Start = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

	[Test]
	public async Task ExhaustedTransientFailureReturnsTheRetryCountOnTheOutcome()
	{
		using var scope = new Scope((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)));

		var task = scope.Client.GetAnimeDetailsOutcomeAsync(1L, TestContext.Current!.Execution.CancellationToken);
		scope.Time.Advance(TimeSpan.FromSeconds(2));
		var outcome = await task;

		var failure = Failed(outcome);
		await Assert.That(failure.Kind).IsEqualTo(TenraiFailureKind.Transport);
		await Assert.That(failure.Facts.RetryCount).IsEqualTo(1);
	}

	[Test]
	public async Task ExhaustedTransientFailureLogsExactlyOneWarningWithTheRetryCount()
	{
		using var scope = new Scope((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)));

		var task = scope.Client.GetAnimeDetailsAsync(1L, TestContext.Current!.Execution.CancellationToken);
		scope.Time.Advance(TimeSpan.FromSeconds(2));
		_ = await task;

		var failure = scope.Logger.Entries.Single(static entry => entry.EventId.Id == TerminalFailureEventId);
		await Assert.That(failure.Level).IsEqualTo(LogLevel.Warning);
		await Assert.That(Field(failure, "RetryCount")).IsEqualTo("1");
		await Assert.That(Field(failure, "Kind")).IsEqualTo(nameof(TenraiFailureKind.Transport));
		await Assert.That(scope.Logger.Entries.Count(static entry => entry.Level == LogLevel.Warning)).IsEqualTo(1);
	}

	[Test]
	public async Task ActiveCooldownReturnsTheRetryAfterThenTheCooldownSuppression()
	{
		using var scope = new Scope((_, _) => Task.FromResult(RateLimited(HttpStatusCode.TooManyRequests, CooldownRetryAfter)));
		var cancellationToken = TestContext.Current!.Execution.CancellationToken;

		var first = await scope.Client.GetAnimeDetailsOutcomeAsync(1L, cancellationToken);
		var second = await scope.Client.GetMangaDetailsOutcomeAsync(SecondMediaId, cancellationToken);

		await Assert.That(Failed(first).Facts.RetryAfter).IsEqualTo(CooldownRetryAfter);
		await Assert.That(Suppression(second)).IsEqualTo(TenraiSuppression.Cooldown);
	}

	[Test]
	public async Task ActiveCooldownSuppressesFollowingOperationsAtDebug()
	{
		using var scope = new Scope((_, _) => Task.FromResult(RateLimited(HttpStatusCode.TooManyRequests, CooldownRetryAfter)));
		var cancellationToken = TestContext.Current!.Execution.CancellationToken;

		_ = await scope.Client.GetAnimeDetailsAsync(1L, cancellationToken);
		_ = await scope.Client.GetMangaDetailsAsync(SecondMediaId, cancellationToken);

		var suppressed = scope.Logger.Entries.Single(static entry => entry.EventId.Id == CooldownSuppressedEventId);
		await Assert.That(suppressed.Level).IsEqualTo(LogLevel.Debug);
		await Assert.That(scope.Logger.Entries.Count(static entry => entry.EventId.Id == TerminalFailureEventId)).IsEqualTo(1);
	}

	[Test]
	public async Task QueueRejectionReturnsTheQueueSuppression()
	{
		using var scope = new Scope(RespondWithEmptyPayload);
		var cancellationToken = TestContext.Current!.Execution.CancellationToken;
		using var queueCancellation = new CancellationTokenSource();
		var queued = await scope.FillQueueAsync(queueCancellation.Token, cancellationToken);

		var outcome = await scope.Client.GetMangaDetailsOutcomeAsync(1L, cancellationToken);

		await Assert.That(Suppression(outcome)).IsEqualTo(TenraiSuppression.Queue);
		await queueCancellation.CancelAsync();
		await Assert.That(async () => await Task.WhenAll(queued)).Throws<OperationCanceledException>();
	}

	[Test]
	public async Task QueueRejectionIsLoggedAtDebug()
	{
		using var scope = new Scope(RespondWithEmptyPayload);
		var cancellationToken = TestContext.Current!.Execution.CancellationToken;
		using var queueCancellation = new CancellationTokenSource();
		var queued = await scope.FillQueueAsync(queueCancellation.Token, cancellationToken);

		_ = await scope.Client.GetMangaDetailsAsync(1L, cancellationToken);

		var rejected = scope.Logger.Entries.Single(static entry => entry.EventId.Id == QueueRejectedEventId);
		await Assert.That(rejected.Level).IsEqualTo(LogLevel.Debug);
		await Assert.That(scope.Logger.Entries.Exists(static entry => entry.Level == LogLevel.Warning)).IsFalse();
		await queueCancellation.CancelAsync();
		await Assert.That(async () => await Task.WhenAll(queued)).Throws<OperationCanceledException>();
	}

	private static Task<HttpResponseMessage> RespondWithEmptyPayload(HttpRequestMessage request, CancellationToken cancellationToken) =>
		Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("{\"data\":{}}"), });

	private static TenraiEnrichmentOutcome<MediaInfo>.Failed Failed(TenraiEnrichmentOutcome<MediaInfo> outcome) =>
		outcome as TenraiEnrichmentOutcome<MediaInfo>.Failed ?? throw new InvalidOperationException("The outcome was not a terminal failure");

	private static TenraiSuppression Suppression(TenraiEnrichmentOutcome<MediaInfo> outcome) =>
		(outcome as TenraiEnrichmentOutcome<MediaInfo>.Suppressed ?? throw new InvalidOperationException("The outcome was not a suppression"))
		.Reason;

	private static HttpResponseMessage RateLimited(HttpStatusCode statusCode, TimeSpan retryAfter)
	{
		var response = new HttpResponseMessage(statusCode);
		response.Headers.RetryAfter = new RetryConditionHeaderValue(retryAfter);
		return response;
	}

	private static string? Field(RecordedLogEntry entry, string name) =>
		entry.State.SingleOrDefault(field => string.Equals(field.Key, name, StringComparison.Ordinal)).Value?.ToString();

	private sealed class Scope : IDisposable
	{
		private readonly FakeHttpMessageHandler _primaryHandler;
		private readonly ServiceProvider _provider;

		public Scope(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond)
		{
			this.Time = new(Start);
			this._primaryHandler = new(respond);
			var services = new ServiceCollection();
			services.AddSingleton<TimeProvider>(this.Time);
			_ = services.AddLogging();
			_ = services.AddTenraiEnrichment();
			_ = services.AddHttpClient(TenraiConstants.HttpClientName).ConfigurePrimaryHttpMessageHandler(() => this._primaryHandler);
			this._provider = services.BuildServiceProvider();
			this.RawClient = this._provider.GetRequiredService<IHttpClientFactory>().CreateClient(TenraiConstants.HttpClientName);
			this.Logger = new();
			this.Client = new(this.Logger, this.RawClient, this._provider.GetRequiredService<TenraiGate>());
		}

		public TenraiEnrichment Client { get; }

		public RecordingLogger<TenraiEnrichment> Logger { get; }

		public HttpClient RawClient { get; }

		public FakeTimeProvider Time { get; }

		public async Task<Task<HttpResponseMessage>[]> FillQueueAsync(CancellationToken queueCancellation, CancellationToken cancellationToken)
		{
			using var first = await this.RawClient.GetAsync("warmup/1", cancellationToken);
			using var second = await this.RawClient.GetAsync("warmup/2", cancellationToken);
			return
			[
				.. Enumerable.Range(0, QueueLimit)
							 .Select(index => this.RawClient.GetAsync("queued/" + index.ToString(CultureInfo.InvariantCulture),
								 queueCancellation)),
			];
		}

		public void Dispose()
		{
			this.RawClient.Dispose();
			this._provider.Dispose();
			this._primaryHandler.Dispose();
		}
	}

	private sealed class FakeHttpMessageHandler(
		Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond) : HttpMessageHandler
	{
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
			respond(request, cancellationToken);
	}
}
