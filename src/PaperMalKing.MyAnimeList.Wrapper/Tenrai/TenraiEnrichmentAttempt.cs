// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

namespace PaperMalKing.MyAnimeList.Wrapper.Tenrai;

internal sealed class TenraiEnrichmentAttempt
{
	private int _retryCount;

	public int RetryCount => Volatile.Read(ref this._retryCount);

	public TimeSpan? RetryAfter { get; private set; }

	public TenraiSuppression Suppression { get; private set; }

	public void RecordRetry() => Interlocked.Increment(ref this._retryCount);

	public void RecordRetryAfter(TimeSpan? retryAfter)
	{
		if (retryAfter is not null)
		{
			this.RetryAfter = retryAfter;
		}
	}

	public void RecordSuppression(TenraiSuppression suppression) => this.Suppression = suppression;
}
