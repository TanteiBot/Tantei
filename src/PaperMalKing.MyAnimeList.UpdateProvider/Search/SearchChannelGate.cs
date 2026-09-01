// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using DSharpPlus;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Search;

internal static class SearchChannelGate
{
	public static bool CanPostEmbed(Permissions permissions, bool isThread) =>
		permissions.HasPermission(Permissions.EmbedLinks) &&
		permissions.HasPermission(isThread ? Permissions.SendMessagesInThreads : Permissions.SendMessages);
}
