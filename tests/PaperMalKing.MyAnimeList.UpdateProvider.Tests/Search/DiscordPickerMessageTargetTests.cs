// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using DSharpPlus.Entities;
using PaperMalKing.MyAnimeList.UpdateProvider.Search;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Tests.Search;

public sealed class DiscordPickerMessageTargetTests
{
	[Test]
	public async Task APublicPostIsSentThroughTheChannelRatherThanTheInteraction()
	{
		var postService = new RecordingPostService();
		var target = new DiscordPickerMessageTarget(_originalInteraction: null!, _channel: null!, postService);
		var embed = new DiscordEmbedBuilder().WithTitle("Monster");

		await target.SendPublicAsync(embed);

		await Assert.That(postService.Embeds).HasSingleItem();
		await Assert.That(postService.Embeds[0]).IsSameReferenceAs(embed);
	}

	private sealed class RecordingPostService : ISearchResultPostService
	{
		public List<DiscordEmbedBuilder> Embeds { get; } = [];

		public Task SendAsync(DiscordChannel channel, DiscordEmbedBuilder embed)
		{
			this.Embeds.Add(embed);
			return Task.CompletedTask;
		}
	}
}
