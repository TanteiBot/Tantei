// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N
using PaperMalKing.Common.Options;

namespace PaperMalKing.AniList.UpdateProvider;

public sealed class AniListOptions : ITimerOptions<AniListUpdateProvider>
{
	public required int DelayBetweenChecksInMilliseconds { get; init; }

	public const string AniList = ProviderConstants.NAME;
}