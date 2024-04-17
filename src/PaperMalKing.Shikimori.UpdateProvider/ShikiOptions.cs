// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.ComponentModel.DataAnnotations;
using PaperMalKing.Common.Options;

namespace PaperMalKing.Shikimori.UpdateProvider;

public sealed class ShikiOptions : ITimerOptions<ShikiUpdateProvider>
{
	public const string Shikimori = Constants.Name;

	[Required]
	[StringLength(int.MaxValue, MinimumLength = 1)]
	public string ShikimoriAppName { get; init; } = null!;

	[Required]
	[Range(0, int.MaxValue)]
	public int DelayBetweenChecksInMilliseconds { get; init; }
}