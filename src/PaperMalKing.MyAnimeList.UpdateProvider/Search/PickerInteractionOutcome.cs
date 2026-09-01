// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

namespace PaperMalKing.MyAnimeList.UpdateProvider.Search;

internal sealed record PickerInteractionOutcome
{
	public static PickerInteractionOutcome Recognized { get; } = new(replacement: null);

	public PickerView? Replacement { get; }

	private PickerInteractionOutcome(PickerView? replacement)
	{
		this.Replacement = replacement;
	}

	public static PickerInteractionOutcome Replace(PickerView replacement)
	{
		ArgumentNullException.ThrowIfNull(replacement);
		return new(replacement);
	}
}
