// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using DSharpPlus.Entities;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Search;

internal sealed class DiscordPickerMessageTarget(
	DiscordInteraction _originalInteraction,
	DiscordChannel _channel,
	ISearchResultPostService _postService) : IPickerMessageTarget
{
	public Task SendPublicAsync(DiscordEmbedBuilder embed, CancellationToken cancellationToken = default) =>
		_postService.SendAsync(_channel, embed, cancellationToken);

	public Task DeleteOriginalAsync(CancellationToken cancellationToken = default) =>
		_originalInteraction.DeleteOriginalResponseAsync().WaitAsync(cancellationToken);

	public Task EditOriginalAsync(PickerView view, CancellationToken cancellationToken = default) =>
		_originalInteraction.EditOriginalResponseAsync(view.ToWebhookBuilder()).WaitAsync(cancellationToken);
}
