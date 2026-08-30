// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

namespace PaperMalKing.MyAnimeList.UpdateProvider.Search;

internal sealed record PickerSearchContext(
	string Query,
	PickerMediaKind MediaKind,
	string? MediaTypeFilter,
	ulong DiscordUserId,
	string RequesterDisplayName,
	string? RequesterAvatarUrl,
	ulong GuildId,
	ulong ChannelId,
	DateTimeOffset InvokedAt);
