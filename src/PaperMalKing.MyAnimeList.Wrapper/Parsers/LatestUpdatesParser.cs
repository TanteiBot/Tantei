// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.Linq;
using System.Text;
using AngleSharp.Dom;
using CommunityToolkit.Diagnostics;
using PaperMalKing.Common.Enums;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions;

namespace PaperMalKing.MyAnimeList.Wrapper.Parsers;

internal static class LatestUpdatesParser
{
	private const string BaseSelectorStart = "div#statistics div.updates.";
	private const string BaseSelectorEnd = " > div";
	private const string AnimeSelector = $"{BaseSelectorStart}anime{BaseSelectorEnd}";
	private const string MangaSelector = $"{BaseSelectorStart}manga{BaseSelectorEnd}";

	public static string? Parse(IDocument document, ListEntryType listEntryType)
	{
		var selector = listEntryType switch
		{
			ListEntryType.Anime => AnimeSelector,
			ListEntryType.Manga => MangaSelector,
			_ => ThrowHelper.ThrowArgumentOutOfRangeException<string>(nameof(listEntryType), listEntryType, message: null),
		};
		var nodes = document.QuerySelectorAll(selector);
		if (nodes is null or [])
		{
			return null;
		}

		return string.Join('|', nodes.Select(dataNode =>
		{
			var link = dataNode.QuerySelector("a")!.GetAttribute("href")!;
			var id = Helper.ExtractIdFromMalUrl(link);

			return
				$"{new StringBuilder(dataNode.QuerySelector("div.data > div:last-of-type")!.TextContent).Replace(" ", "").Replace("\n", "").ToString().ToUpperInvariant()}:{id}";
		}));
	}
}