﻿// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

namespace PaperMalKing.Common.Options;

public interface IRateLimitOptions<T>
{
	/// <summary>
	/// Gets amount of requests that can be permitted.
	/// </summary>
	int AmountOfRequests { get; init; }

	/// <summary>
	/// Gets amount of time in milliseconds between refreshing number of permits.
	/// </summary>
	int PeriodInMilliseconds { get; init; }
}