// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

namespace PaperMalKing.MyAnimeList.UpdateProvider.Search;

internal enum MatchRank : byte
{
	Primary = 0,
	Synonym = 1,
	Japanese = 2,
	English = 3,
	Contains = 4,
	None = 5,
}
