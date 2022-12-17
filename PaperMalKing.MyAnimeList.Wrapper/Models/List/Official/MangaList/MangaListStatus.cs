// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

namespace PaperMalKing.MyAnimeList.Wrapper.Models.List.Official.MangaList;

internal enum MangaListStatus : byte
{
	unknown = 0,
	reading = 1,
	completed = 2,
	on_hold = 3,
	dropped = 4,
	plan_to_read = 5
}