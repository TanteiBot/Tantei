// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

namespace PaperMalKing.MyAnimeList.Wrapper.Models.List.Official.AnimeList;

internal enum AnimeAiringStatus : byte
{
	uknown = 0,
	finished_airing = 1,
	currently_airing = 2,
	not_yet_aired = 3
}