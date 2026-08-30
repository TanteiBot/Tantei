// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

namespace PaperMalKing.MyAnimeList.UpdateProvider.Search;

internal sealed record SearchEvaluation(
	SearchOutcomeKind Kind,
	IReadOnlyList<PickerSearchResult> Results,
	int FloorSurvivorCount,
	PickerSearchResult? AutoPostResult)
{
	public static SearchEvaluation From<TResult>(SearchOutcome<TResult> outcome, Func<RankedSearchResult<TResult>, PickerSearchResult> project)
		where TResult : class
	{
		ArgumentNullException.ThrowIfNull(outcome);
		ArgumentNullException.ThrowIfNull(project);
		return new(
			outcome.Kind,
			[.. outcome.Results.Select(project)],
			outcome.FloorSurvivorCount,
			outcome.AutoPostResult is null ? null : project(outcome.AutoPostResult));
	}
}
