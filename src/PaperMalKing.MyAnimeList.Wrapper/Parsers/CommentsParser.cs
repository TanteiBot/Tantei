// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using HtmlAgilityPack;

namespace PaperMalKing.MyAnimeList.Wrapper.Parsers;

internal static class CommentsParser
{
	internal static string Parse(HtmlNode node)
	{
		var text = node.SelectSingleNode("//a[contains(@href,'profile')]").GetAttributeValue("href",null);
		return text.Substring(text.LastIndexOf('/') + 1);
	}
}