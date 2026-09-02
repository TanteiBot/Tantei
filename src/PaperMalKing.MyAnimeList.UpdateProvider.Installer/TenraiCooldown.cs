// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Net;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Installer;

internal sealed class TenraiCooldown
{
	private readonly Lock _gate = new();
	private readonly TimeProvider _timeProvider;
	private DateTimeOffset _expiresAt;

	public TenraiCooldown(TimeProvider timeProvider)
	{
		ArgumentNullException.ThrowIfNull(timeProvider);
		this._timeProvider = timeProvider;
	}

	public bool IsActive
	{
		get
		{
			lock (this._gate)
			{
				return this._timeProvider.GetUtcNow() < this._expiresAt;
			}
		}
	}

	public TimeSpan? GetRetryAfter(HttpResponseMessage response)
	{
		ArgumentNullException.ThrowIfNull(response);
		if (response.StatusCode is not (HttpStatusCode.TooManyRequests or HttpStatusCode.ServiceUnavailable))
		{
			return null;
		}

		return this.ParseRetryAfter(response);
	}

	public TimeSpan? ApplyRetryAfter(HttpResponseMessage response)
	{
		var delay = this.GetRetryAfter(response);
		if (delay is null)
		{
			return null;
		}

		lock (this._gate)
		{
			var now = this._timeProvider.GetUtcNow();
			var expiresAt = delay.Value >= DateTimeOffset.MaxValue - now ? DateTimeOffset.MaxValue : now + delay.Value;
			if (expiresAt > this._expiresAt)
			{
				this._expiresAt = expiresAt;
			}
		}

		return delay;
	}

	private TimeSpan? ParseRetryAfter(HttpResponseMessage response)
	{
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
}
