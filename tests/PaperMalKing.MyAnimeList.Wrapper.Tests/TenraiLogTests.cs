// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using Microsoft.Extensions.Logging;
using PaperMalKing.MyAnimeList.Wrapper.Tenrai;

namespace PaperMalKing.MyAnimeList.Wrapper.Tests;

public sealed class TenraiLogTests
{
	private const int TerminalFailureEventId = 1;
	private const int NotFoundEventId = 2;
	private const int CircuitSkippedEventId = 3;
	private const int CooldownSuppressedEventId = 4;
	private const int QueueRejectedEventId = 5;
	private const int CircuitOpenedEventId = 6;
	private const int CircuitClosedEventId = 7;
	private const string Anime = "anime";
	private const long MediaId = 5114L;
	private const int Status = 503;
	private const int RetryCount = 1;
	private const long ElapsedMilliseconds = 4200L;
	private const double OpenDurationSeconds = 30D;
	private static readonly TimeSpan RetryAfter = TimeSpan.FromSeconds(3);

	[Test]
	public async Task TerminalFailureIsWarningWithEveryRequiredField()
	{
		foreach (var kind in new[] { TenraiFailureKind.Transport, TenraiFailureKind.Schema })
		{
			var logger = new RecordingLogger<MyAnimeListClient>();

			logger.TenraiEnrichmentFailed(Anime, MediaId, kind, Status, RetryCount, ElapsedMilliseconds, RetryAfter);

			var entry = logger.Single();
			await Assert.That(entry.Level).IsEqualTo(LogLevel.Warning);
			await Assert.That(entry.EventId.Id).IsEqualTo(TerminalFailureEventId);
			await Assert.That(Field(entry, "Operation")).IsEqualTo(Anime);
			await Assert.That(Field(entry, "MediaId")).IsEqualTo("5114");
			await Assert.That(Field(entry, "Kind")).IsEqualTo(kind.ToString());
			await Assert.That(Field(entry, "Status")).IsEqualTo("503");
			await Assert.That(Field(entry, "RetryCount")).IsEqualTo("1");
			await Assert.That(Field(entry, "ElapsedMilliseconds")).IsEqualTo("4200");
			await Assert.That(Field(entry, "RetryAfter")).IsEqualTo("00:00:03");
		}
	}

	[Test]
	public async Task TerminalFailureKeepsOptionalFieldsWhenAbsent()
	{
		var logger = new RecordingLogger<MyAnimeListClient>();

		logger.TenraiEnrichmentFailed("characters", 1L, TenraiFailureKind.Transport, status: null, retryCount: 0, elapsedMilliseconds: 1L, retryAfter: null);

		var entry = logger.Single();
		await Assert.That(entry.Level).IsEqualTo(LogLevel.Warning);
		await Assert.That(HasField(entry, "Status")).IsTrue();
		await Assert.That(RawField(entry, "Status") is null).IsTrue();
		await Assert.That(HasField(entry, "RetryAfter")).IsTrue();
		await Assert.That(RawField(entry, "RetryAfter") is null).IsTrue();
		await Assert.That(Field(entry, "RetryCount")).IsEqualTo("0");
	}

	[Test]
	public async Task TerminalFailureCarriesNoExceptionAndNoResponseBody()
	{
		const string body = "SECRET-PROVIDER-BODY-7f3a";
		var logger = new RecordingLogger<MyAnimeListClient>();

		logger.TenraiEnrichmentFailed("manga", 1L, TenraiFailureKind.Schema, Status, retryCount: 0, elapsedMilliseconds: 1L, retryAfter: null);

		var entry = logger.Single();
		await Assert.That(entry.Exception is null).IsTrue();
		await Assert.That(entry.Message.Contains(body, StringComparison.Ordinal)).IsFalse();
	}

	[Test]
	public async Task NotFoundIsDebug()
	{
		var logger = new RecordingLogger<MyAnimeListClient>();

		logger.TenraiEnrichmentNotFound(Anime, MediaId);

		var entry = logger.Single();
		await Assert.That(entry.Level).IsEqualTo(LogLevel.Debug);
		await Assert.That(entry.EventId.Id).IsEqualTo(NotFoundEventId);
		await Assert.That(Field(entry, "Operation")).IsEqualTo(Anime);
		await Assert.That(Field(entry, "MediaId")).IsEqualTo("5114");
	}

	[Test]
	public async Task CircuitSkippedIsDebug()
	{
		var logger = new RecordingLogger<MyAnimeListClient>();

		logger.TenraiEnrichmentCircuitSkipped("manga", MediaId);

		var entry = logger.Single();
		await Assert.That(entry.Level).IsEqualTo(LogLevel.Debug);
		await Assert.That(entry.EventId.Id).IsEqualTo(CircuitSkippedEventId);
	}

	[Test]
	public async Task CooldownSuppressedIsDebug()
	{
		var logger = new RecordingLogger<MyAnimeListClient>();

		logger.TenraiEnrichmentCooldownSuppressed("characters", MediaId);

		var entry = logger.Single();
		await Assert.That(entry.Level).IsEqualTo(LogLevel.Debug);
		await Assert.That(entry.EventId.Id).IsEqualTo(CooldownSuppressedEventId);
	}

	[Test]
	public async Task QueueRejectedIsDebug()
	{
		var logger = new RecordingLogger<MyAnimeListClient>();

		logger.TenraiEnrichmentQueueRejected(Anime, MediaId);

		var entry = logger.Single();
		await Assert.That(entry.Level).IsEqualTo(LogLevel.Debug);
		await Assert.That(entry.EventId.Id).IsEqualTo(QueueRejectedEventId);
	}

	[Test]
	public async Task CircuitOpenedIsWarningWithDuration()
	{
		var logger = new RecordingLogger<TenraiCircuit>();

		logger.TenraiCircuitOpened(OpenDurationSeconds);

		var entry = logger.Single();
		await Assert.That(entry.Level).IsEqualTo(LogLevel.Warning);
		await Assert.That(entry.EventId.Id).IsEqualTo(CircuitOpenedEventId);
		await Assert.That(Field(entry, "OpenDurationSeconds")).IsEqualTo("30");
	}

	[Test]
	public async Task CircuitClosedIsWarning()
	{
		var logger = new RecordingLogger<TenraiCircuit>();

		logger.TenraiCircuitClosed();

		var entry = logger.Single();
		await Assert.That(entry.Level).IsEqualTo(LogLevel.Warning);
		await Assert.That(entry.EventId.Id).IsEqualTo(CircuitClosedEventId);
	}

	private static string? Field(RecordedLogEntry entry, string name) => RawField(entry, name)?.ToString();

	private static object? RawField(RecordedLogEntry entry, string name) =>
		entry.State.SingleOrDefault(field => string.Equals(field.Key, name, StringComparison.Ordinal)).Value;

	private static bool HasField(RecordedLogEntry entry, string name) =>
		entry.State.Any(field => string.Equals(field.Key, name, StringComparison.Ordinal));
}
