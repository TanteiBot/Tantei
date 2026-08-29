// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

namespace PaperMalKing.MyAnimeList.UpdateProvider.Search;

internal enum SearchOutcomeKind : byte
{
	NoResults = 0,
	TypeFilterEmpty = 1,
	AutoPost = 2,
	Picker = 3,
}
