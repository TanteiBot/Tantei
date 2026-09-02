// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

namespace PaperMalKing.MyAnimeList.Wrapper.Tenrai;

internal sealed class TenraiCircuit
{
	private const int FailureThreshold = 5;
	private static readonly TimeSpan OpenDuration = TimeSpan.FromSeconds(30);
	private static readonly TimeSpan RollingWindow = TimeSpan.FromSeconds(30);
	private readonly Queue<DateTimeOffset> _failures = new();
	private readonly Lock _gate = new();
	private readonly TimeProvider _timeProvider;
	private DateTimeOffset _openUntil;

	public TenraiCircuit(TimeProvider timeProvider)
	{
		ArgumentNullException.ThrowIfNull(timeProvider);
		this._timeProvider = timeProvider;
	}

	public bool IsOpen
	{
		get
		{
			lock (this._gate)
			{
				return this._timeProvider.GetUtcNow() < this._openUntil;
			}
		}
	}

	public void RecordTerminalFailure()
	{
		lock (this._gate)
		{
			var now = this._timeProvider.GetUtcNow();
			if (now < this._openUntil)
			{
				return;
			}

			this.PruneExpired(now);
			this._failures.Enqueue(now);
			if (this._failures.Count < FailureThreshold)
			{
				return;
			}

			this._openUntil = now + OpenDuration;
			this._failures.Clear();
		}
	}

	private void PruneExpired(DateTimeOffset now)
	{
		var cutoff = now - RollingWindow;
		while (this._failures.TryPeek(out var timestamp) && timestamp <= cutoff)
		{
			_ = this._failures.Dequeue();
		}
	}
}
