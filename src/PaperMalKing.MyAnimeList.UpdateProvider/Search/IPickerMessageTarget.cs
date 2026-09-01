// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using DSharpPlus.Entities;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Search;

internal interface IPickerMessageTarget
{
	Task SendPublicAsync(DiscordEmbedBuilder embed, CancellationToken cancellationToken = default);

	Task DeleteOriginalAsync(CancellationToken cancellationToken = default);

	Task EditOriginalAsync(PickerView view, CancellationToken cancellationToken = default);
}
