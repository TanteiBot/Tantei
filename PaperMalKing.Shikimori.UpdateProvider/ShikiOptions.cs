// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using PaperMalKing.Common.Options;

namespace PaperMalKing.Shikimori.UpdateProvider;

internal sealed class ShikiOptions : ITimerOptions<ShikiUpdateProvider>
{
	public const string Shikimori = Constants.NAME;

	public string ShikimoriAppName { get; init; } = null!;
		
	/// <inheritdoc />
	public int DelayBetweenChecksInMilliseconds { get; init; }
}