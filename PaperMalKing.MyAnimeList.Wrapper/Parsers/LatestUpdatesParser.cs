﻿// SPDX-License-Identifier: AGPL-3.0-or-later
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

	internal static string? Parse(HtmlNode node, ListEntryType listEntryType)
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
		var dataText = dataNode.SelectSingleNode("//div[1]/div[2]").InnerText.Replace(" ", "", StringComparison.Ordinal);
		Debug.Assert(dataText.Length < 100, "We rely on progress string being small");

		Span<byte> shaHashDestination = stackalloc byte[SHA256.HashSizeInBytes];
		Span<byte> utf8Destination = stackalloc byte[Encoding.UTF8.GetMaxByteCount(dataText.Length)];
		Encoding.UTF8.GetBytes(dataText, utf8Destination);
		SHA256.HashData(utf8Destination, shaHashDestination);

		return Convert.ToHexString(shaHashDestination);
	}
}