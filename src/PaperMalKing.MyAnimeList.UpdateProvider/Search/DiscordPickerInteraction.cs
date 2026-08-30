// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Search;

internal sealed class DiscordPickerInteraction(ComponentInteractionCreateEventArgs _event) : IPickerInteraction
{
	public string CustomId => _event.Id;

	public IReadOnlyList<string> Values => _event.Values;

	public ulong DiscordUserId => _event.User.Id;

	public string DiscordDisplayName => _event.User is DiscordMember member ? member.DisplayName : _event.User.Username;

	public ulong? GuildId => _event.Guild?.Id;

	public ulong? ChannelId => _event.Channel?.Id;

	public bool HasAcknowledged { get; private set; }

	public async Task UpdateAsync(PickerView view)
	{
		await _event.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, view.ToInteractionResponseBuilder()).ConfigureAwait(false);
		this.HasAcknowledged = true;
	}

	public async Task DeferAsync()
	{
		await _event.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate).ConfigureAwait(false);
		this.HasAcknowledged = true;
	}

	public Task EditAsync(PickerView view) => _event.Interaction.EditOriginalResponseAsync(view.ToWebhookBuilder());
}
