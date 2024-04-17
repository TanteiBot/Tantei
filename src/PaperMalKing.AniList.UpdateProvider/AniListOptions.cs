// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.ComponentModel.DataAnnotations;
using PaperMalKing.Common.Options;

namespace PaperMalKing.AniList.UpdateProvider;

public sealed class AniListOptions : ITimerOptions<AniListUpdateProvider>
{
	[Required]
	[Range(0, int.MaxValue)]
	public int DelayBetweenChecksInMilliseconds { get; init; }

	public const string AniList = ProviderConstants.Name;
}