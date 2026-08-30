// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using Humanizer;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.AnimeList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.MangaList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Search;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Search;

internal sealed record SearchEvaluation(
	SearchOutcomeKind Kind,
	IReadOnlyList<SearchResult> Results,
	int FloorSurvivorCount,
	SearchResult? AutoPostResult)
{
	public static SearchEvaluation Evaluate(
		MatchKey queryKey,
		AnimeSearchResponse response,
		AnimeMediaType? mediaTypeFilter)
	{
		ArgumentNullException.ThrowIfNull(response);
		return Evaluate(
			queryKey,
			response.Results.Select(static envelope => envelope.Result),
			mediaTypeFilter,
			static result => result.MediaType,
			AdaptAnime);
	}

	public static SearchEvaluation Evaluate(
		MatchKey queryKey,
		MangaSearchResponse response,
		MangaMediaType? mediaTypeFilter)
	{
		ArgumentNullException.ThrowIfNull(response);
		return Evaluate(
			queryKey,
			response.Results.Select(static envelope => envelope.Result),
			mediaTypeFilter,
			static result => result.MediaType,
			AdaptManga);
	}

	private static SearchEvaluation Evaluate<TResult, TMediaType>(
		MatchKey queryKey,
		IEnumerable<TResult> results,
		TMediaType? mediaTypeFilter,
		Func<TResult, TMediaType> getMediaType,
		Func<TResult, MatchRank, SearchResult> adapt)
		where TResult : BaseSearchResult
		where TMediaType : struct, Enum
	{
		ArgumentNullException.ThrowIfNull(queryKey);
		ArgumentNullException.ThrowIfNull(results);
		if (queryKey.IsEmpty)
		{
			throw new ArgumentException("The query Match Key cannot be empty.", nameof(queryKey));
		}

		var floorSurvivors = results
			.Select(result => (Result: result, Rank: GetMatchRank(queryKey, result)))
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
			.Select(result => adapt(result.Result, result.Rank))
			.OrderBy(static result => result.Rank)
			.ThenByDescending(static result => result.ListUserCount)
			.ThenBy(static result => result.Id)
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

	private static MatchRank GetMatchRank(MatchKey queryKey, BaseSearchResult result)
	{
		var candidates = CreateCandidateKeys(result);
		var exactMatch = candidates.Find(candidate => candidate.Key.Equals(queryKey));
		if (exactMatch != default)
		{
			return exactMatch.Rank;
		}

		return candidates.Exists(candidate => candidate.Key.Contains(queryKey)) ? MatchRank.Contains : MatchRank.None;
	}

	private static List<(MatchKey Key, MatchRank Rank)> CreateCandidateKeys(BaseSearchResult result)
	{
		var candidates = new List<(MatchKey Key, MatchRank Rank)>();
		var keys = new HashSet<MatchKey>();
		Add(result.PrimaryTitle, MatchRank.Primary);
		if (result.AlternativeTitles?.Synonyms is { } synonyms)
		{
			foreach (var synonym in synonyms)
			{
				Add(synonym, MatchRank.Synonym);
			}
		}

		Add(result.AlternativeTitles?.Japanese, MatchRank.Japanese);
		Add(result.AlternativeTitles?.English, MatchRank.English);
		return candidates;

		void Add(string? title, MatchRank exactRank)
		{
			if (title is null)
			{
				return;
			}

			var key = MatchKey.Create(title);
			if (!key.IsEmpty && keys.Add(key))
			{
				candidates.Add((key, exactRank));
			}
		}
	}

	private static SearchResult AdaptAnime(AnimeSearchResult result, MatchRank rank) => new(
		result.Id,
		result.PrimaryTitle,
		rank,
		context => SearchEmbedBuilder.Build(result, context.RequesterDisplayName, context.RequesterAvatarUrl))
	{
		MediaKind = PickerMediaKind.Anime,
		MediaType = result.MediaType.Humanize(LetterCasing.Sentence),
		Year = result.StartSeason is { Year: not 0U } startSeason ? startSeason.Year : null,
		Mean = result.Mean,
		ListUserCount = result.ListUserCount,
	};

	private static SearchResult AdaptManga(MangaSearchResult result, MatchRank rank) => new(
		result.Id,
		result.PrimaryTitle,
		rank,
		context => SearchEmbedBuilder.Build(result, context.RequesterDisplayName, context.RequesterAvatarUrl))
	{
		MediaKind = PickerMediaKind.Manga,
		MediaType = result.MediaType.Humanize(LetterCasing.Sentence),
		Mean = result.Mean,
		ListUserCount = result.ListUserCount,
	};
}
