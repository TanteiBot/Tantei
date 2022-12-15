// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using PaperMalKing.Common.Options;

namespace PaperMalKing.AniList.UpdateProvider;

internal sealed class AniListOptions : ITimerOptions<AniListUpdateProvider>
{
	public required int DelayBetweenChecksInMilliseconds { get; init; }

	public const string AniList = Constants.NAME;
}