// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics;
using System.Net;
using Microsoft.Extensions.Logging;

namespace PaperMalKing.MyAnimeList.Wrapper.Tenrai;

internal sealed class TenraiGate
{
	private const int FailureThreshold = 5;
	private static readonly TimeSpan OpenDuration = TimeSpan.FromSeconds(30);
	private static readonly TimeSpan RollingWindow = TimeSpan.FromSeconds(30);
	private readonly Queue<DateTimeOffset> _failures = new();
	private readonly ILogger<TenraiGate> _logger;
	private readonly Lock _sync = new();
	private readonly TimeProvider _timeProvider;
	private DateTimeOffset _cooldownUntil;
	private bool _open;
	private DateTimeOffset _openUntil;

	public TenraiGate(TimeProvider timeProvider, ILogger<TenraiGate> logger)
	{
		ArgumentNullException.ThrowIfNull(timeProvider);
		ArgumentNullException.ThrowIfNull(logger);
		this._timeProvider = timeProvider;
		this._logger = logger;
	}

	public TenraiSuppression? Check()
	{
		bool open;
		bool cooling;
		TenraiCircuitTransition transition;
		lock (this._sync)
		{
			var now = this._timeProvider.GetUtcNow();
			(open, transition) = this.RefreshLocked(now);
			cooling = now < this._cooldownUntil;
		}

		this.ReportTransition(transition);
		if (open)
		{
			return TenraiSuppression.CircuitOpen;
		}

		return cooling ? TenraiSuppression.Cooldown : null;
	}

	public TimeSpan? Record(TenraiSignal signal)
	{
		switch (signal.Kind)
		{
			case TenraiSignalKind.Attempted:
				return signal.Response is { } attempted ? this.EngageCooldown(attempted) : null;
			case TenraiSignalKind.Completed:
				if (signal.Response is { } completed && CountsTowardCircuit(completed.StatusCode))
				{
					this.RecordTerminalFailure();
				}

				return null;
			case TenraiSignalKind.Failed:
				this.RecordTerminalFailure();
				return null;
			default:
				throw new UnreachableException();
		}
	}

	private static bool CountsTowardCircuit(HttpStatusCode statusCode) => statusCode is
		HttpStatusCode.RequestTimeout or
		HttpStatusCode.InternalServerError or
		HttpStatusCode.BadGateway or
		HttpStatusCode.ServiceUnavailable or
		HttpStatusCode.GatewayTimeout;

	private TimeSpan? EngageCooldown(HttpResponseMessage response)
	{
		var delay = this.ParseRetryAfter(response);
		if (delay is null)
		{
			return null;
		}

		var engaged = false;
		lock (this._sync)
		{
			var now = this._timeProvider.GetUtcNow();
			var wasActive = now < this._cooldownUntil;
			var expiresAt = delay.Value >= DateTimeOffset.MaxValue - now ? DateTimeOffset.MaxValue : now + delay.Value;
			if (expiresAt > this._cooldownUntil)
			{
				this._cooldownUntil = expiresAt;
				engaged = !wasActive;
			}
		}

		if (engaged)
		{
			this._logger.TenraiCooldownEngaged(delay.Value);
		}

		return delay;
	}

	private TimeSpan? ParseRetryAfter(HttpResponseMessage response)
	{
		if (response.StatusCode is not (HttpStatusCode.TooManyRequests or HttpStatusCode.ServiceUnavailable))
		{
			return null;
		}

		try
		{
			var retryAfter = response.Headers.RetryAfter;
			if (retryAfter?.Delta is { } delta)
			{
				return delta >= TimeSpan.Zero ? delta : null;
			}

			if (retryAfter?.Date is not { } date)
			{
				return null;
			}

			var delay = date - this._timeProvider.GetUtcNow();
			return delay >= TimeSpan.Zero ? delay : null;
		}
		catch (FormatException)
		{
			return null;
		}
	}

	private void RecordTerminalFailure()
	{
		TenraiCircuitTransition transition;
		lock (this._sync)
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

		this.ReportTransition(transition);
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

	private void ReportTransition(TenraiCircuitTransition transition)
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
