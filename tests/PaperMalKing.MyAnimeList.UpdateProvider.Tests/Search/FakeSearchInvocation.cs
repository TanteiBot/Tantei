// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using PaperMalKing.UpdatesProviders.Base.Search;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Tests.Search;

internal sealed record FakeSearchInvocation(IPickerMessageTarget Target) : ISearchInvocation
{
	public bool CanPostEmbed { get; init; } = true;

	public bool IncludeNsfw { get; init; }

	public ulong DiscordUserId { get; init; } = 1UL;

	public string RequesterDisplayName { get; init; } = "Requester";

	public string? RequesterAvatarUrl { get; init; }

	public ulong GuildId { get; init; } = 2UL;

	public ulong ChannelId { get; init; } = 3UL;
}
