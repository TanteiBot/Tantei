// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using Microsoft.Extensions.Logging;
using PaperMalKing.MyAnimeList.Wrapper.Tenrai;

namespace PaperMalKing.MyAnimeList.Wrapper.Tests;

public sealed class TenraiCircuitLoggingTests
{
	private const int CircuitOpenedEventId = 6;
	private const int CircuitClosedEventId = 7;
	private const int FailureThreshold = 5;
	private static readonly DateTimeOffset Start = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);

	[Test]
	public async Task FailuresBelowThresholdEmitNoTransitionLog()
	{
		var logger = new RecordingLogger<TenraiCircuit>();
		var circuit = new TenraiCircuit(new TestTimeProvider(Start), logger);

		for (var failure = 0; failure < FailureThreshold - 1; failure++)
		{
			circuit.RecordTerminalFailure();
		}

		await Assert.That(logger.Entries).IsEmpty();
	}

	[Test]
	public async Task OpeningEmitsExactlyOneWarning()
	{
		var logger = new RecordingLogger<TenraiCircuit>();
		var circuit = new TenraiCircuit(new TestTimeProvider(Start), logger);

		for (var failure = 0; failure < FailureThreshold; failure++)
		{
			circuit.RecordTerminalFailure();
		}

		var opened = logger.Entries.Single();
		await Assert.That(opened.Level).IsEqualTo(LogLevel.Warning);
		await Assert.That(opened.EventId.Id).IsEqualTo(CircuitOpenedEventId);
	}

	[Test]
	public async Task ClosingAfterTheWindowEmitsExactlyOneClosedWarning()
	{
		var logger = new RecordingLogger<TenraiCircuit>();
		var time = new TestTimeProvider(Start);
		var circuit = new TenraiCircuit(time, logger);
		for (var failure = 0; failure < FailureThreshold; failure++)
		{
			circuit.RecordTerminalFailure();
		}

		time.Advance(TimeSpan.FromSeconds(30));
		_ = circuit.IsOpen;
		_ = circuit.IsOpen;

		await Assert.That(logger.Entries.Count(static entry => entry.EventId.Id == CircuitOpenedEventId)).IsEqualTo(1);
		var closed = logger.Entries.Single(static entry => entry.EventId.Id == CircuitClosedEventId);
		await Assert.That(closed.Level).IsEqualTo(LogLevel.Warning);
	}

	[Test]
	public async Task FailuresWhileOpenDoNotEmitAdditionalOpenWarnings()
	{
		var logger = new RecordingLogger<TenraiCircuit>();
		var time = new TestTimeProvider(Start);
		var circuit = new TenraiCircuit(time, logger);
		for (var failure = 0; failure < FailureThreshold; failure++)
		{
			circuit.RecordTerminalFailure();
		}

		time.Advance(TimeSpan.FromSeconds(10));
		circuit.RecordTerminalFailure();

		await Assert.That(logger.Entries.Count(static entry => entry.EventId.Id == CircuitOpenedEventId)).IsEqualTo(1);
		await Assert.That(logger.Entries.Exists(static entry => entry.EventId.Id == CircuitClosedEventId)).IsFalse();
	}

	private sealed class TestTimeProvider(DateTimeOffset start) : TimeProvider
	{
		private DateTimeOffset _now = start;

		public override DateTimeOffset GetUtcNow() => this._now;

		public void Advance(TimeSpan amount) => this._now += amount;
	}
}
