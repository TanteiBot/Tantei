// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using DSharpPlus;
using DSharpPlus.EventArgs;

namespace PaperMalKing.UpdatesProviders.Base.Search;

internal sealed class SearchPickerComponentHandler(DiscordClient _client, SearchPicker _picker) : IExecuteOnStartupService
{
	private int _activated;

	internal int ActivationCount { get; private set; }

	public Task ExecuteAsync(CancellationToken cancellationToken)
	{
		cancellationToken.ThrowIfCancellationRequested();
		if (Interlocked.Exchange(ref this._activated, 1) == 0)
		{
			_client.ComponentInteractionCreated += this.HandleAsync;
			this.ActivationCount++;
		}

		return Task.CompletedTask;
	}

	private Task<bool> HandleAsync(DiscordClient _, ComponentInteractionCreateEventArgs eventArgs) =>
		_picker.HandleAsync(new DiscordPickerInteraction(eventArgs));
}
