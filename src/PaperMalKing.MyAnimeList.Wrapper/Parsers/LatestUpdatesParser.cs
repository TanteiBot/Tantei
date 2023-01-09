// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using HtmlAgilityPack;
using PaperMalKing.Common.Enums;

namespace PaperMalKing.MyAnimeList.Wrapper.Parsers;

internal static class LatestUpdatesParser
{
	private const string BaseSelectorStart = "//div[contains(@class, 'updates ";
	private const string BaseSelectorEnd = "')]/div[1]";
	private const string AnimeSelector = $"{BaseSelectorStart}anime{BaseSelectorEnd}";
	private const string MangaSelector = $"{BaseSelectorStart}manga{BaseSelectorEnd}";

	public static string? Parse(HtmlNode node, ListEntryType listEntryType)
	{
		var selector = listEntryType switch
		{
			ListEntryType.Anime => AnimeSelector,
			ListEntryType.Manga => MangaSelector,
			_ => throw new ArgumentOutOfRangeException(nameof(listEntryType), listEntryType, null)
		};
		var dataNode = node.SelectSingleNode(selector);
		if (dataNode is null)
			return null;
		var hd = new HtmlDocument();
		hd.LoadHtml(dataNode.InnerHtml);
		dataNode = hd.DocumentNode;
		var link = dataNode.SelectSingleNode("//a").Attributes["href"].Value;
		var id = CommonParser.ExtractIdFromMalUrl(link);

		var dataText = $"{new StringBuilder(dataNode.SelectSingleNode("//div[1]/div[2]").InnerText).Replace(" ", "").Replace("\n", "").ToString().ToUpperInvariant()}::{id}" ;

		return dataText;
	}
}