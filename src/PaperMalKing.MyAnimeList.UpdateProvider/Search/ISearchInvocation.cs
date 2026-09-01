// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

namespace PaperMalKing.MyAnimeList.UpdateProvider.Search;

internal interface ISearchInvocation
{
	bool CanPostEmbed { get; }

	bool IncludeNsfw { get; }

	ulong DiscordUserId { get; }

	string RequesterDisplayName { get; }

	string? RequesterAvatarUrl { get; }

	ulong GuildId { get; }

	ulong ChannelId { get; }

	IPickerMessageTarget Target { get; }
}
