// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.ComponentModel.DataAnnotations;
using PaperMalKing.Common.Options;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions;

namespace PaperMalKing.MyAnimeList.UpdateProvider;

internal sealed class MalOptions : IRateLimitOptions<IMyAnimeListClient>, ITimerOptions<MalUpdateProvider>
{
	public const string MyAnimeList = Constants.Name;

	[Required]
	[Range(0, int.MaxValue)]
	public int AmountOfRequests { get; init; }

	[Required]
	[Range(0, int.MaxValue)]
	public int PeriodInMilliseconds { get; init; }

	[Required]
	[Range(0, int.MaxValue)]
	public int DelayBetweenChecksInMilliseconds { get; init; }

	[Required]
	[StringLength(int.MaxValue, MinimumLength = 1)]
	public string ClientId { get; init; } = null!;
}