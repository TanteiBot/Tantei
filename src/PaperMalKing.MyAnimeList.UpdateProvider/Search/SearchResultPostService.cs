// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using DSharpPlus.Entities;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Search;

internal sealed class SearchResultPostService : ISearchResultPostService
{
	public Task SendAsync(DiscordChannel channel, DiscordEmbedBuilder embed, CancellationToken cancellationToken = default)
	{
		ArgumentNullException.ThrowIfNull(channel);
		ArgumentNullException.ThrowIfNull(embed);
		return SendCoreAsync(channel, embed, cancellationToken);
	}

	private static async Task SendCoreAsync(DiscordChannel channel, DiscordEmbedBuilder embed, CancellationToken cancellationToken)
	{
		_ = await channel.SendMessageAsync(embed: embed).WaitAsync(cancellationToken).ConfigureAwait(false);
	}
}
