// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using PaperMalKing.Common.Enums;
using static PaperMalKing.MyAnimeList.Wrapper.Constants;

namespace PaperMalKing.MyAnimeList.Wrapper.Models.List.Types;

internal abstract class AnimeListType : IListType<AnimeListEntry>
{
	public static string LatestUpdatesUrl(string username) => ANIME_LIST_URL + username + LATEST_LIST_UPDATES;

	public static ListEntryType ListEntryType => ListEntryType.Anime;
}