// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using Microsoft.Extensions.Logging;

namespace PaperMalKing.MyAnimeList.Wrapper.Tenrai;

internal sealed class TenraiCircuit
{
	private const int FailureThreshold = 5;
	private static readonly TimeSpan OpenDuration = TimeSpan.FromSeconds(30);
	private static readonly TimeSpan RollingWindow = TimeSpan.FromSeconds(30);
	private readonly Queue<DateTimeOffset> _failures = new();
	private readonly Lock _gate = new();
	private readonly ILogger<TenraiCircuit> _logger;
	private readonly TimeProvider _timeProvider;
	private bool _open;
	private DateTimeOffset _openUntil;

	public TenraiCircuit(TimeProvider timeProvider, ILogger<TenraiCircuit> logger)
	{
		ArgumentNullException.ThrowIfNull(timeProvider);
		ArgumentNullException.ThrowIfNull(logger);
		this._timeProvider = timeProvider;
		this._logger = logger;
	}

	public bool IsOpen
	{
		get
		{
			bool open;
			TenraiCircuitTransition transition;
			lock (this._gate)
			{
				(open, transition) = this.RefreshLocked(this._timeProvider.GetUtcNow());
			}

			this.Report(transition);
			return open;
		}
	}

	public void RecordTerminalFailure()
	{
		TenraiCircuitTransition transition;
		lock (this._gate)
		{
			var now = this._timeProvider.GetUtcNow();
			bool open;
			(open, transition) = this.RefreshLocked(now);
			if (open)
			{
				return;
			}

			this.PruneExpired(now);
			this._failures.Enqueue(now);
			if (this._failures.Count >= FailureThreshold)
			{
				this._openUntil = now + OpenDuration;
				this._open = true;
				this._failures.Clear();
				transition = TenraiCircuitTransition.Opened;
			}
		}

		this.Report(transition);
	}

	private (bool Open, TenraiCircuitTransition Transition) RefreshLocked(DateTimeOffset now)
	{
		if (!this._open)
		{
			return (false, TenraiCircuitTransition.None);
		}

		if (now < this._openUntil)
		{
			return (true, TenraiCircuitTransition.None);
		}

		this._open = false;
		return (false, TenraiCircuitTransition.Closed);
	}

	private void Report(TenraiCircuitTransition transition)
	{
		if (transition is TenraiCircuitTransition.Opened)
		{
			this._logger.TenraiCircuitOpened(OpenDuration.TotalSeconds);
			return;
		}

		if (transition is TenraiCircuitTransition.Closed)
		{
			this._logger.TenraiCircuitClosed();
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

	private enum TenraiCircuitTransition
	{
		None = 0,
		Opened = 1,
		Closed = 2,
	}
}
