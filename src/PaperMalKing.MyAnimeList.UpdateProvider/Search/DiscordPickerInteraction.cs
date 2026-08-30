// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Search;

internal sealed class DiscordPickerInteraction(ComponentInteractionCreateEventArgs eventArgs) : IPickerInteraction
{
	private readonly ComponentInteractionCreateEventArgs _eventArgs = eventArgs;
	private bool _hasAcknowledged;

	public string CustomId => this._eventArgs.Id;

	public IReadOnlyList<string> Values => this._eventArgs.Values;

	public ulong DiscordUserId => this._eventArgs.User.Id;

	public string DiscordDisplayName => this._eventArgs.User is DiscordMember member ? member.DisplayName : this._eventArgs.User.Username;

	public ulong? GuildId => this._eventArgs.Guild?.Id;

	public ulong? ChannelId => this._eventArgs.Channel?.Id;

	public Task ApplyOutcomeAsync(PickerInteractionOutcome outcome, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(outcome);
		return this.ApplyOutcomeCoreAsync(outcome, cancellationToken);
	}

	private async Task ApplyOutcomeCoreAsync(PickerInteractionOutcome outcome, CancellationToken cancellationToken)
	{
		if (outcome.Replacement is null)
		{
			if (!this._hasAcknowledged)
			{
				await this._eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate).ConfigureAwait(false);
				this._hasAcknowledged = true;
			}

			return;
		}

		if (!this._hasAcknowledged)
		{
			await this._eventArgs.Interaction.CreateResponseAsync(
				InteractionResponseType.UpdateMessage,
				outcome.Replacement.ToInteractionResponseBuilder()).ConfigureAwait(false);
			this._hasAcknowledged = true;
			return;
		}

		await this._eventArgs.Interaction.EditOriginalResponseAsync(outcome.Replacement.ToWebhookBuilder())
			.WaitAsync(cancellationToken)
			.ConfigureAwait(false);
	}
}
