// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.Collections.Generic;
using DSharpPlus.Entities;

namespace PaperMalKing.UpdatesProviders.Base;

public sealed class BaseUpdate : IUpdate
{
	private readonly IReadOnlyList<DiscordEmbedBuilder> _updates = [];

	private readonly IAsyncEnumerable<DiscordEmbedBuilder> _asyncUpdates = EmptyAsync();

	public BaseUpdate(IReadOnlyList<DiscordEmbedBuilder> updateEmbeds)
	{
		this._updates = updateEmbeds;
	}

	public BaseUpdate(IAsyncEnumerable<DiscordEmbedBuilder> updates)
	{
		this._asyncUpdates = updates;
	}

	public async IAsyncEnumerable<DiscordEmbed> GetUpdateEmbedsAsync()
	{
		foreach (var embed in this._updates)
		{
			yield return embed.Build();
		}

		await foreach (var embed in this._asyncUpdates)
		{
			yield return embed.Build();
		}
	}

#pragma warning disable CS1998
	// Async method lacks 'await' operators and will run synchronously
	private static async IAsyncEnumerable<DiscordEmbedBuilder> EmptyAsync()
#pragma warning restore
	{
		yield break;
	}
}