// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using HtmlAgilityPack;
using PaperMalKing.Common.Enums;
using PaperMalKing.MyAnimeList.Wrapper.Models;
using PaperMalKing.MyAnimeList.Wrapper.Models.Favorites;

namespace PaperMalKing.MyAnimeList.Wrapper.Parsers;

internal static partial class UserProfileParser
{
	internal static User Parse(HtmlNode node, ParserOptions options)
	{
		var reportUrl = node.SelectSingleNode("//a[contains(@class, 'header-right')]").Attributes["href"].Value;
		var li = reportUrl.LastIndexOf('=');

		var id = uint.Parse(reportUrl.Substring(li + 1));
		var url = node.SelectSingleNode("//meta[@property='og:url']").Attributes["content"].Value;
		var username = url.Substring(url.LastIndexOf('/') + 1);
		var favorites = options.HasFlag(ParserOptions.Favorites) ? FavoritesParser.Parse(node) : UserFavorites.Empty;

		return new()
		{
			Favorites = favorites,
			Username = username,
			Id = id,
			LatestAnimeUpdateHash = options.HasFlag(ParserOptions.AnimeList) ? LatestUpdatesParser.Parse(node, ListEntryType.Anime) : null,
			LatestMangaUpdateHash = options.HasFlag(ParserOptions.MangaList) ? LatestUpdatesParser.Parse(node, ListEntryType.Manga) : null
		};
	}
}