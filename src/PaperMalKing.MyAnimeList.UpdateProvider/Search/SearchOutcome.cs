// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

namespace PaperMalKing.MyAnimeList.UpdateProvider.Search;

internal sealed record SearchOutcome<TResult>(
	SearchOutcomeKind Kind,
	IReadOnlyList<RankedSearchResult<TResult>> Results,
	int FloorSurvivorCount,
	RankedSearchResult<TResult>? AutoPostResult)
	where TResult : class;
