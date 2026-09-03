// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using DSharpPlus.Entities;

namespace PaperMalKing.UpdatesProviders.Base.Search;

internal sealed class DiscordPickerMessageTarget(
	DiscordInteraction _originalInteraction,
	DiscordChannel _channel) : IPickerMessageTarget
{
	public async Task SendPublicAsync(DiscordEmbedBuilder embed, CancellationToken cancellationToken = default)
	{
		_ = await _channel.SendMessageAsync(embed: embed).WaitAsync(cancellationToken).ConfigureAwait(false);
	}

	public Task DeleteOriginalAsync(CancellationToken cancellationToken = default) =>
		_originalInteraction.DeleteOriginalResponseAsync().WaitAsync(cancellationToken);

	public Task EditOriginalAsync(PickerView view, CancellationToken cancellationToken = default) =>
		_originalInteraction.EditOriginalResponseAsync(view.ToWebhookBuilder()).WaitAsync(cancellationToken);
}
