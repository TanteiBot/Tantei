// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using PaperMalKing.Common.Enums;

namespace PaperMalKing.MyAnimeList.Wrapper.Models.Rss.Types;

internal abstract class AnimeRssFeed : IRssFeedType
{
	public static string Url => Constants.RSS_ANIME_URL;

	public static ListEntryType Type => ListEntryType.Anime;
}