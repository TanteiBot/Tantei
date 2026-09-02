// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
using System.Threading.RateLimiting;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Installer;

internal sealed class TenraiRateLimiter : RateLimiter
{
	private const int QueueLimit = 10;
	private const int TokenLimit = 2;
	private static readonly TimeSpan ReplenishmentPeriod = TimeSpan.FromSeconds(2.4D);
	private readonly Lock _gate = new();
	private readonly LinkedList<PendingRequest> _queue = [];
	private readonly ITimer _timer;
	private long _failedLeases;
	private long _successfulLeases;
	private int _tokens = TokenLimit;
	private bool _disposed;

	[SuppressMessage("Major Bug", "S3366:Make sure 'this' is not exposed", Justification = "The timer cannot fire before construction completes")]
	public TenraiRateLimiter(TimeProvider timeProvider)
	{
		ArgumentNullException.ThrowIfNull(timeProvider);
		this._timer = timeProvider.CreateTimer(static state => ((TenraiRateLimiter)state!).Replenish(), this,
			ReplenishmentPeriod, ReplenishmentPeriod);
	}

	public override TimeSpan? IdleDuration => null;

	public override RateLimiterStatistics GetStatistics()
	{
		lock (this._gate)
		{
			return new()
			{
				CurrentAvailablePermits = this._tokens,
				CurrentQueuedCount = this._queue.Count,
				TotalFailedLeases = this._failedLeases,
				TotalSuccessfulLeases = this._successfulLeases,
			};
		}
	}

	protected override RateLimitLease AttemptAcquireCore(int permitCount)
	{
		if (permitCount is 0)
		{
			return TenraiRateLimitLease.Acquired;
		}

		lock (this._gate)
		{
			this.ThrowIfDisposed();
			if (this.TryAcquireImmediately(permitCount, out var lease))
			{
				return lease;
			}

			this._failedLeases++;
			return TenraiRateLimitLease.Rejected;
		}
	}

	protected override ValueTask<RateLimitLease> AcquireAsyncCore(int permitCount, CancellationToken cancellationToken)
	{
		if (permitCount is 0)
		{
			return ValueTask.FromResult<RateLimitLease>(TenraiRateLimitLease.Acquired);
		}

		lock (this._gate)
		{
			this.ThrowIfDisposed();
			if (permitCount is not 1)
			{
				this._failedLeases++;
				return ValueTask.FromResult<RateLimitLease>(TenraiRateLimitLease.Rejected);
			}

			if (this.TryAcquireImmediately(permitCount, out var lease))
			{
				return ValueTask.FromResult(lease);
			}

			if (this._queue.Count >= QueueLimit)
			{
				this._failedLeases++;
				return ValueTask.FromResult<RateLimitLease>(TenraiRateLimitLease.Rejected);
			}

			var pending = new PendingRequest(cancellationToken);
			pending.Node = this._queue.AddLast(pending);
			pending.CancellationRegistration = cancellationToken.Register(static state =>
			{
				var registration = (CancellationState)state!;
				registration.Limiter.Cancel(registration.Request);
			}, new CancellationState(this, pending));
			return new(pending.Completion.Task);
		}
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		if (!disposing)
		{
			return;
		}

		PendingRequest[] pending;
		lock (this._gate)
		{
			if (this._disposed)
			{
				return;
			}

			this._disposed = true;
			pending = [.. this._queue];
			this._queue.Clear();
		}

		this._timer.Dispose();
		foreach (var request in pending)
		{
			request.CancellationRegistration.Dispose();
			request.Completion.TrySetException(new ObjectDisposedException(nameof(TenraiRateLimiter)));
		}
	}

	private void Cancel(PendingRequest request)
	{
		lock (this._gate)
		{
			if (request.Node?.List is null)
			{
				return;
			}

			this._queue.Remove(request.Node);
			request.Node = null;
			this._failedLeases++;
		}

		request.Completion.TrySetCanceled(request.CancellationToken);
	}

	private void Replenish()
	{
		PendingRequest? pending = null;
		lock (this._gate)
		{
			if (this._disposed)
			{
				return;
			}

			if (this._queue.First is { } first)
			{
				pending = first.Value;
				this._queue.RemoveFirst();
				pending.Node = null;
				this._successfulLeases++;
			}
			else
			{
				this._tokens = Math.Min(TokenLimit, this._tokens + 1);
			}
		}

		if (pending is not null)
		{
			pending.CancellationRegistration.Dispose();
			pending.Completion.TrySetResult(TenraiRateLimitLease.Acquired);
		}
	}

	private bool TryAcquireImmediately(int permitCount, out RateLimitLease lease)
	{
		if (permitCount is 1 && this._tokens > 0 && this._queue.Count is 0)
		{
			this._tokens--;
			this._successfulLeases++;
			lease = TenraiRateLimitLease.Acquired;
			return true;
		}

		lease = TenraiRateLimitLease.Rejected;
		return false;
	}

	private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(this._disposed, this);

	private sealed class PendingRequest(CancellationToken cancellationToken)
	{
		public CancellationToken CancellationToken { get; } = cancellationToken;

		public CancellationTokenRegistration CancellationRegistration { get; set; }

		public TaskCompletionSource<RateLimitLease> Completion { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

		public LinkedListNode<PendingRequest>? Node { get; set; }
	}

	private sealed record CancellationState(TenraiRateLimiter Limiter, PendingRequest Request);

	private sealed class TenraiRateLimitLease(bool isAcquired) : RateLimitLease
	{
		public static TenraiRateLimitLease Acquired { get; } = new(isAcquired: true);

		public static TenraiRateLimitLease Rejected { get; } = new(isAcquired: false);

		public override bool IsAcquired { get; } = isAcquired;

		public override IEnumerable<string> MetadataNames => [];

		public override bool TryGetMetadata(string metadataName, out object? metadata)
		{
			metadata = null;
			return false;
		}
	}
}
