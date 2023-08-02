// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N
using System;

namespace PaperMalKing.Common.RateLimiters;

public sealed class RateLimit
{
	public static readonly RateLimit Empty = new();

	public int AmountOfRequests { get; }

	public int PeriodInMilliseconds { get; }

	public RateLimit(int amountOfRequests, int periodInMilliseconds)
	{
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amountOfRequests);
		ArgumentOutOfRangeException.ThrowIfNegativeOrZero(periodInMilliseconds);
		this.AmountOfRequests = amountOfRequests;
		this.PeriodInMilliseconds = periodInMilliseconds;
	}

	public RateLimit(int amountOfRequests, TimeSpan period) : this(amountOfRequests, (int)period.TotalMilliseconds)
	{ }

	private RateLimit()
	{
		this.PeriodInMilliseconds = 0;
		this.AmountOfRequests = 0;
	}

	public override string ToString()
	{
		return $"{this.AmountOfRequests}r per {this.PeriodInMilliseconds}ms";
	}
}