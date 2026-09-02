// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using PaperMalKing.MyAnimeList.Wrapper.Tenrai;

namespace PaperMalKing.MyAnimeList.Wrapper.Tests;

public sealed class MyAnimeListClientLoggingTests
{
	private const int TerminalFailureEventId = 1;
	private const int NotFoundEventId = 2;
	private const int CircuitSkippedEventId = 3;
	private const int FailureThreshold = 5;
	private const long AnimeId = 1L;
	private const long MangaId = 2L;
	private const string SecretBody = "SECRET-PROVIDER-BODY-7f3a";

	[Test]
	public async Task MalformedSuccessLogsOneWarningWithoutTheResponseBody()
	{
		using var scope = new ClientScope((_, _) => Task.FromResult(JsonResponse(HttpStatusCode.OK, "{\"data\":\"" + SecretBody + "\"}")));

		_ = await scope.Client.GetAnimeDetailsAsync(AnimeId, TestContext.Current!.Execution.CancellationToken);

		var failure = Failures(scope.Logger).Single();
		await Assert.That(failure.Level).IsEqualTo(LogLevel.Warning);
		await Assert.That(failure.Exception is null).IsTrue();
		await Assert.That(Field(failure, "Kind")).IsEqualTo(nameof(TenraiFailureKind.Schema));
		await AssertNoEntryMentionsTheBody(scope.Logger);
	}

	[Test]
	public async Task TransportFailureLogsOneWarningWithStatusAndNoBody()
	{
		using var scope = new ClientScope((_, _) => Task.FromResult(JsonResponse(HttpStatusCode.InternalServerError, "{\"message\":\"" + SecretBody + "\"}")));

		_ = await scope.Client.GetMangaDetailsAsync(MangaId, TestContext.Current!.Execution.CancellationToken);

		var failure = Failures(scope.Logger).Single();
		await Assert.That(failure.Level).IsEqualTo(LogLevel.Warning);
		await Assert.That(Field(failure, "Kind")).IsEqualTo(nameof(TenraiFailureKind.Transport));
		await Assert.That(Field(failure, "Status")).IsEqualTo("500");
		await AssertNoEntryMentionsTheBody(scope.Logger);
	}

	[Test]
	public async Task NotFoundIsLoggedAtDebugAndNeverAsWarning()
	{
		using var scope = new ClientScope((_, _) => Task.FromResult(JsonResponse(HttpStatusCode.NotFound, "{\"message\":\"missing\"}")));

		_ = await scope.Client.GetAnimeDetailsAsync(AnimeId, TestContext.Current!.Execution.CancellationToken);

		await Assert.That(scope.Logger.Entries.Exists(static entry => entry.Level == LogLevel.Warning)).IsFalse();
		var notFound = scope.Logger.Entries.Single(static entry => entry.EventId.Id == NotFoundEventId);
		await Assert.That(notFound.Level).IsEqualTo(LogLevel.Debug);
	}

	[Test]
	public async Task CallerCancellationEmitsNoFailureLog()
	{
		using var cancellationSource = new CancellationTokenSource();
		cancellationSource.Cancel();
		var expected = new OperationCanceledException(cancellationSource.Token);
		using var scope = new ClientScope((_, _) => Task.FromException<HttpResponseMessage>(expected));

		try
		{
			_ = await scope.Client.GetAnimeSeiyuAsync(AnimeId, cancellationSource.Token);
		}
		catch (OperationCanceledException exception)
		{
			_ = exception;
		}

		await Assert.That(scope.Logger.Entries.Exists(static entry => entry.Level >= LogLevel.Warning)).IsFalse();
		await Assert.That(Failures(scope.Logger)).IsEmpty();
	}

	[Test]
	public async Task OpenCircuitIsLoggedAtDebugWithoutCallingTheProvider()
	{
		var requests = 0;
		using var scope = new ClientScope((_, _) =>
		{
			Interlocked.Increment(ref requests);
			return Task.FromResult(JsonResponse(HttpStatusCode.OK, "{\"data\":{}}"));
		});
		for (var failure = 0; failure < FailureThreshold; failure++)
		{
			scope.Circuit.RecordTerminalFailure();
		}

		var requestsBefore = Volatile.Read(ref requests);
		_ = await scope.Client.GetAnimeDetailsAsync(AnimeId, TestContext.Current!.Execution.CancellationToken);

		await Assert.That(Volatile.Read(ref requests)).IsEqualTo(requestsBefore);
		var skipped = scope.Logger.Entries.Single(static entry => entry.EventId.Id == CircuitSkippedEventId);
		await Assert.That(skipped.Level).IsEqualTo(LogLevel.Debug);
		await Assert.That(Failures(scope.Logger)).IsEmpty();
	}

	[Test]
	public async Task SuccessfulEnrichmentEmitsNoFailureLog()
	{
		using var scope = new ClientScope((_, _) => Task.FromResult(JsonResponse(HttpStatusCode.OK, "{\"data\":{\"themes\":[{\"name\":\"Action\"}]}}")));

		var result = await scope.Client.GetAnimeDetailsAsync(AnimeId, TestContext.Current!.Execution.CancellationToken);

		await Assert.That(result.Themes).HasSingleItem();
		await Assert.That(scope.Logger.Entries.Exists(static entry => entry.Level >= LogLevel.Warning)).IsFalse();
		await Assert.That(Failures(scope.Logger)).IsEmpty();
	}

	private static RecordedLogEntry[] Failures(RecordingLogger<MyAnimeListClient> logger) =>
		[.. logger.Entries.Where(static entry => entry.EventId.Id == TerminalFailureEventId)];

	private static async Task AssertNoEntryMentionsTheBody(RecordingLogger<MyAnimeListClient> logger)
	{
		foreach (var entry in logger.Entries)
		{
			await Assert.That(entry.Message.Contains(SecretBody, StringComparison.Ordinal)).IsFalse();
			foreach (var field in entry.State)
			{
				var rendered = field.Value?.ToString() ?? string.Empty;
				await Assert.That(rendered.Contains(SecretBody, StringComparison.Ordinal)).IsFalse();
			}
		}
	}

	private static string? Field(RecordedLogEntry entry, string name) =>
		entry.State.SingleOrDefault(field => string.Equals(field.Key, name, StringComparison.Ordinal)).Value?.ToString();

	private static HttpResponseMessage JsonResponse(HttpStatusCode statusCode, string json) => new(statusCode)
	{
		Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"),
	};

	private sealed class ClientScope : IDisposable
	{
		private readonly FakeHttpMessageHandler _handler;
		private readonly HttpClient _tenraiClient;

		public ClientScope(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond)
		{
			this._handler = new(respond);
			this._tenraiClient = new(this._handler, disposeHandler: false)
			{
				BaseAddress = new("https://example.test/v1/"),
			};
			this.Logger = new();
			this.Circuit = new(new FixedTimeProvider(), NullLogger<TenraiCircuit>.Instance);
			this.Client = new(this.Logger, null!, null!, this._tenraiClient, this.Circuit);
		}

		public TenraiCircuit Circuit { get; }

		public MyAnimeListClient Client { get; }

		public RecordingLogger<MyAnimeListClient> Logger { get; }

		public void Dispose()
		{
			this._tenraiClient.Dispose();
			this._handler.Dispose();
		}
	}

	private sealed class FakeHttpMessageHandler(
		Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> respond) : HttpMessageHandler
	{
		protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
			respond(request, cancellationToken);
	}

	private sealed class FixedTimeProvider : TimeProvider
	{
		private static readonly DateTimeOffset Now = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

		public override DateTimeOffset GetUtcNow() => Now;
	}
}
