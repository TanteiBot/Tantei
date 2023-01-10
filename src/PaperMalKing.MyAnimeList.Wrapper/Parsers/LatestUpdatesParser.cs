// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System;
using System.Text;
using AngleSharp.Dom;
using PaperMalKing.Common.Enums;

namespace PaperMalKing.MyAnimeList.Wrapper.Parsers;

internal static class LatestUpdatesParser
{
	private const string BaseSelectorStart = "div.updates.";
	private const string BaseSelectorEnd = " > div:first-of-type";
	private const string AnimeSelector = $"{BaseSelectorStart}anime{BaseSelectorEnd}";
	private const string MangaSelector = $"{BaseSelectorStart}manga{BaseSelectorEnd}";

	public static string? Parse(IDocument document, ListEntryType listEntryType)
	{
		var selector = listEntryType switch
		{
			ListEntryType.Anime => AnimeSelector,
			ListEntryType.Manga => MangaSelector,
			_ => throw new ArgumentOutOfRangeException(nameof(listEntryType), listEntryType, null)
		};
		var dataNode = document.QuerySelector(selector);
		if (dataNode is null)
			return null;
		var link = dataNode.QuerySelector("a")!.GetAttribute("href")!;
		var id = CommonParser.ExtractIdFromMalUrl(link);

		var dataText = $"{new StringBuilder(dataNode.QuerySelector("div.data > div:last-of-type")!.TextContent).Replace(" ", "").Replace("\n", "").ToString().ToUpperInvariant()}::{id}" ;

		return dataText;
	}
}