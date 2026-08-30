// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.AnimeList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.MangaList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Search;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Search;

internal static class SearchPipeline
{
	public static SearchOutcome<AnimeSearchResult> Evaluate(
		MatchKey queryKey,
		AnimeSearchResponse response,
		AnimeMediaType? mediaTypeFilter) =>
		Evaluate(
			queryKey,
			response.Results.Select(static envelope => envelope.Result),
			mediaTypeFilter,
			static result => result.MediaType,
			static result => result.ListUserCount,
			static result => result.Id,
			SearchResultRanker.GetMatchRank);

	public static SearchOutcome<MangaSearchResult> Evaluate(
		MatchKey queryKey,
		MangaSearchResponse response,
		MangaMediaType? mediaTypeFilter) =>
		Evaluate(
			queryKey,
			response.Results.Select(static envelope => envelope.Result),
			mediaTypeFilter,
			static result => result.MediaType,
			static result => result.ListUserCount,
			static result => result.Id,
			SearchResultRanker.GetMatchRank);

	private static SearchOutcome<TResult> Evaluate<TResult, TMediaType>(
		MatchKey queryKey,
		IEnumerable<TResult> results,
		TMediaType? mediaTypeFilter,
		Func<TResult, TMediaType> getMediaType,
		Func<TResult, uint> getListUserCount,
		Func<TResult, uint> getId,
		Func<MatchKey, TResult, MatchRank> getMatchRank)
		where TResult : class
		where TMediaType : struct, Enum
	{
		ArgumentNullException.ThrowIfNull(queryKey);
		ArgumentNullException.ThrowIfNull(results);
		var floorSurvivors = results
			.Select(result => new RankedSearchResult<TResult>(result, getMatchRank(queryKey, result)))
			.Where(static result => result.Rank != MatchRank.None)
			.ToArray();
		if (floorSurvivors.Length == 0)
		{
			return new(
				Kind: SearchOutcomeKind.NoResults,
				Results: [],
				FloorSurvivorCount: 0,
				AutoPostResult: null);
		}

		var filtered = mediaTypeFilter.HasValue
			? floorSurvivors.Where(result => EqualityComparer<TMediaType>.Default.Equals(getMediaType(result.Result), mediaTypeFilter.Value))
			: floorSurvivors;
		var sorted = filtered
			.OrderBy(static result => result.Rank)
			.ThenByDescending(result => getListUserCount(result.Result))
			.ThenBy(result => getId(result.Result))
			.ToArray();
		var rankedResults = Array.AsReadOnly(sorted);
		if (rankedResults.Count == 0)
		{
			return new(
				Kind: SearchOutcomeKind.TypeFilterEmpty,
				Results: rankedResults,
				FloorSurvivorCount: floorSurvivors.Length,
				AutoPostResult: null);
		}

		var primaryMatches = rankedResults.Where(static result => result.Rank == MatchRank.Primary).Take(2).ToArray();
		if (primaryMatches.Length == 1)
		{
			return new(SearchOutcomeKind.AutoPosted, rankedResults, floorSurvivors.Length, primaryMatches[0]);
		}

		if (rankedResults.Count == 1)
		{
			return new(SearchOutcomeKind.AutoPosted, rankedResults, floorSurvivors.Length, rankedResults[0]);
		}

		return new(
			Kind: SearchOutcomeKind.PickerOpened,
			Results: rankedResults,
			FloorSurvivorCount: floorSurvivors.Length,
			AutoPostResult: null);
	}
}
