// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using PaperMalKing.MyAnimeList.Wrapper.Tenrai;

namespace PaperMalKing.MyAnimeList.Wrapper.Tests;

public sealed class TenraiCircuitTests
{
	private const int FailureThreshold = 5;
	private static readonly DateTimeOffset Start = new(2026, 1, 1, 0, 0, 0, TimeSpan.Zero);
	private static readonly TimeSpan Window = TimeSpan.FromSeconds(30);

	[Test]
	public async Task FourFailuresWithinTheWindowKeepTheCircuitClosed()
	{
		var time = new TestTimeProvider(Start);
		var circuit = new TenraiCircuit(time);

		for (var failure = 0; failure < FailureThreshold - 1; failure++)
		{
			circuit.RecordTerminalFailure();
		}

		await Assert.That(circuit.IsOpen).IsFalse();
	}

	[Test]
	public async Task FifthFailureWithinTheWindowOpensTheCircuit()
	{
		var time = new TestTimeProvider(Start);
		var circuit = new TenraiCircuit(time);

		for (var failure = 0; failure < FailureThreshold; failure++)
		{
			circuit.RecordTerminalFailure();
		}

		await Assert.That(circuit.IsOpen).IsTrue();
	}

	[Test]
	public async Task FailuresSpreadAcrossTheWindowStillAccumulate()
	{
		var time = new TestTimeProvider(Start);
		var circuit = new TenraiCircuit(time);

		for (var failure = 0; failure < FailureThreshold - 1; failure++)
		{
			circuit.RecordTerminalFailure();
		}

		time.Advance(Window - TimeSpan.FromSeconds(1));
		await Assert.That(circuit.IsOpen).IsFalse();
		circuit.RecordTerminalFailure();

		await Assert.That(circuit.IsOpen).IsTrue();
	}

	[Test]
	public async Task FailuresOlderThanTheWindowAreEvicted()
	{
		var time = new TestTimeProvider(Start);
		var circuit = new TenraiCircuit(time);
		for (var failure = 0; failure < FailureThreshold - 1; failure++)
		{
			circuit.RecordTerminalFailure();
		}

		time.Advance(Window + TimeSpan.FromSeconds(1));
		for (var failure = 0; failure < FailureThreshold - 1; failure++)
		{
			circuit.RecordTerminalFailure();
		}

		await Assert.That(circuit.IsOpen).IsFalse();
		circuit.RecordTerminalFailure();
		await Assert.That(circuit.IsOpen).IsTrue();
	}

	[Test]
	public async Task OpenCircuitClosesAfterThirtySeconds()
	{
		var time = new TestTimeProvider(Start);
		var circuit = new TenraiCircuit(time);
		for (var failure = 0; failure < FailureThreshold; failure++)
		{
			circuit.RecordTerminalFailure();
		}

		time.Advance(TimeSpan.FromSeconds(30) - TimeSpan.FromTicks(1));
		await Assert.That(circuit.IsOpen).IsTrue();
		time.Advance(TimeSpan.FromTicks(1));

		await Assert.That(circuit.IsOpen).IsFalse();
	}

	[Test]
	public async Task FailuresRecordedWhileOpenDoNotExtendTheOpenWindow()
	{
		var time = new TestTimeProvider(Start);
		var circuit = new TenraiCircuit(time);
		for (var failure = 0; failure < FailureThreshold; failure++)
		{
			circuit.RecordTerminalFailure();
		}

		time.Advance(TimeSpan.FromSeconds(15));
		circuit.RecordTerminalFailure();
		time.Advance(TimeSpan.FromSeconds(15));

		await Assert.That(circuit.IsOpen).IsFalse();
	}

	private sealed class TestTimeProvider(DateTimeOffset start) : TimeProvider
	{
		private DateTimeOffset _now = start;

		public override DateTimeOffset GetUtcNow() => this._now;

		public void Advance(TimeSpan amount) => this._now += amount;
	}
}
