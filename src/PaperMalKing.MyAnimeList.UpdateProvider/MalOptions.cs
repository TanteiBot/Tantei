// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using PaperMalKing.Common.Options;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions;

namespace PaperMalKing.MyAnimeList.UpdateProvider;

internal sealed class MalOptions : IRateLimitOptions<IMyAnimeListClient>, ITimerOptions<MalUpdateProvider>
{
	public const string MyAnimeList = Constants.Name;

	public required int AmountOfRequests { get; init; }

	public required int PeriodInMilliseconds { get; init; }

	public required int DelayBetweenChecksInMilliseconds { get; init; }

	public required string ClientId { get; init; }
}