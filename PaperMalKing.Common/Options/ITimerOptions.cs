// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
namespace PaperMalKing.Common.Options
{
	public interface ITimerOptions<T>
	{
		int DelayBetweenChecksInMilliseconds { get; }
	}
}