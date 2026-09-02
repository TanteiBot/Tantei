// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Net;
using Microsoft.Extensions.Logging;

namespace PaperMalKing.MyAnimeList.Wrapper.Tenrai;

internal sealed class TenraiCooldown
{
	private readonly Lock _gate = new();
	private readonly ILogger<TenraiCooldown> _logger;
	private readonly TimeProvider _timeProvider;
	private DateTimeOffset _expiresAt;

	public TenraiCooldown(TimeProvider timeProvider, ILogger<TenraiCooldown> logger)
	{
		ArgumentNullException.ThrowIfNull(timeProvider);
		ArgumentNullException.ThrowIfNull(logger);
		this._timeProvider = timeProvider;
		this._logger = logger;
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

		var engaged = false;
		lock (this._gate)
		{
			var now = this._timeProvider.GetUtcNow();
			var wasActive = now < this._expiresAt;
			var expiresAt = delay.Value >= DateTimeOffset.MaxValue - now ? DateTimeOffset.MaxValue : now + delay.Value;
			if (expiresAt > this._expiresAt)
			{
				this._expiresAt = expiresAt;
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
