// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using DSharpPlus;
using PaperMalKing.MyAnimeList.UpdateProvider.Search;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Tests.Search;

public sealed class SearchChannelGateTests
{
	[Test]
	[Arguments(Permissions.EmbedLinks | Permissions.SendMessages, false, true)]
	[Arguments(Permissions.SendMessages, false, false)]
	[Arguments(Permissions.EmbedLinks, false, false)]
	[Arguments(Permissions.EmbedLinks | Permissions.SendMessagesInThreads, false, false)]
	[Arguments(Permissions.EmbedLinks | Permissions.SendMessagesInThreads, true, true)]
	[Arguments(Permissions.EmbedLinks | Permissions.SendMessages, true, false)]
	[Arguments(Permissions.Administrator, true, true)]
	[Arguments(Permissions.Administrator, false, true)]
	[Arguments(Permissions.None, true, false)]
	public async Task AChannelKindDecidesWhichSendPermissionIsRequired(Permissions permissions, bool isThread, bool expected)
	{
		await Assert.That(SearchChannelGate.CanPostEmbed(permissions, isThread)).IsEqualTo(expected);
	}
}
