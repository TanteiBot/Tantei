// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.Globalization;
using AngleSharp.Dom;
using PaperMalKing.Common.Enums;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Favorites;
using System;

namespace PaperMalKing.MyAnimeList.Wrapper.Parsers;

internal static partial class UserProfileParser
{
	public static User Parse(IDocument document, ParserOptions options)
	{
		var reportUrl = document.QuerySelector("#contentWrapper a.header-right")!.GetAttribute("href");
		var li = reportUrl!.LastIndexOf('=');

		var id = uint.Parse(reportUrl.AsSpan(li + 1), NumberFormatInfo.InvariantInfo);
		var url = document.QuerySelector("meta[property=\"og:url\"]")!.GetAttribute("content")!;
		var username = url[(url.LastIndexOf('/') + 1)..];
		var favorites = options.HasFlag(ParserOptions.Favorites) ? FavoritesParser.Parse(document) : UserFavorites.Empty;

		return new()
		{
			Favorites = favorites,
			Username = username,
			Id = id,
			LatestAnimeUpdateHash = options.HasFlag(ParserOptions.AnimeList) ? LatestUpdatesParser.Parse(document, ListEntryType.Anime) : null,
			LatestMangaUpdateHash = options.HasFlag(ParserOptions.MangaList) ? LatestUpdatesParser.Parse(document, ListEntryType.Manga) : null,
		};
	}
}