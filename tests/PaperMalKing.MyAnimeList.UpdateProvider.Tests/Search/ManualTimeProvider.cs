// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

namespace PaperMalKing.MyAnimeList.UpdateProvider.Tests.Search;

internal sealed class ManualTimeProvider(DateTimeOffset now) : TimeProvider
{
	private readonly Lock _gate = new();
	private readonly List<ManualTimer> _timers = [];
	private DateTimeOffset _now = now;

	public override DateTimeOffset GetUtcNow() => this._now;

	public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
	{
		var timer = new ManualTimer(this, callback, state, dueTime, period);
		lock (this._gate)
		{
			this._timers.Add(timer);
		}

		return timer;
	}

	public void Advance(TimeSpan amount)
	{
		ManualTimer[] due;
		lock (this._gate)
		{
			this._now += amount;
			due = [.. this._timers.Where(timer => timer.IsDue(this._now))];
		}

		foreach (var timer in due)
		{
			timer.Fire(this._now);
		}
	}

	private sealed class ManualTimer(
		ManualTimeProvider _provider,
		TimerCallback _callback,
		object? _state,
		TimeSpan dueTime,
		TimeSpan period) : ITimer
	{
		private DateTimeOffset? _dueAt = dueTime == Timeout.InfiniteTimeSpan ? null : _provider.GetUtcNow() + dueTime;
		private TimeSpan _period = period;
		private bool _disposed;

		public bool Change(TimeSpan dueTime, TimeSpan period)
		{
			lock (_provider._gate)
			{
				if (this._disposed)
				{
					return false;
				}

				this._period = period;
				this._dueAt = dueTime == Timeout.InfiniteTimeSpan ? null : _provider.GetUtcNow() + dueTime;
				return true;
			}
		}

		public bool IsDue(DateTimeOffset now)
		{
			lock (_provider._gate)
			{
				return !this._disposed && this._dueAt <= now;
			}
		}

		public void Fire(DateTimeOffset now)
		{
			lock (_provider._gate)
			{
				if (this._disposed || this._dueAt > now)
				{
					return;
				}

				this._dueAt = this._period == Timeout.InfiniteTimeSpan ? null : now + this._period;
			}

			_callback(_state);
		}

		public void Dispose()
		{
			lock (_provider._gate)
			{
				this._disposed = true;
				this._dueAt = null;
			}
		}

		public ValueTask DisposeAsync()
		{
			this.Dispose();
			return ValueTask.CompletedTask;
		}
	}
}
