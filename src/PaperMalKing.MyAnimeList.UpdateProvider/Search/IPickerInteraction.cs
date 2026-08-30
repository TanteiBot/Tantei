// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

namespace PaperMalKing.MyAnimeList.UpdateProvider.Search;

internal interface IPickerInteraction
{
	string CustomId { get; }

	IReadOnlyList<string> Values { get; }

	ulong DiscordUserId { get; }

	bool HasAcknowledged { get; }

	Task UpdateAsync(PickerView view);

	Task DeferAsync();

	Task EditAsync(PickerView view);
}
