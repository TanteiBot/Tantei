// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using PaperMalKing.Common.Enums;

namespace PaperMalKing.MyAnimeList.Wrapper.Models.Rss.Types
{
	internal interface IRssFeedType
	{
		abstract static string Url { get; }

		abstract static ListEntryType Type { get; }

		internal abstract class Anime : IRssFeedType
		{
			public static string Url => Constants.RSS_ANIME_URL;

			public static ListEntryType Type => ListEntryType.Anime;
		}
	}
}