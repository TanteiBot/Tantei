// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Globalization;
using Humanizer;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Search;
using PaperMalKing.UpdatesProviders.Base.Search;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Search;

internal static class MalMediaEvaluator
{
	private const uint Thousand = 1_000U;
	private const uint Million = 1_000_000U;

	public static SearchEvaluation Evaluate<TMediaType, TStatus>(
		MatchKey queryKey,
		IEnumerable<BaseSearchResult<TMediaType, TStatus>> results,
		TMediaType? mediaTypeFilter)
		where TMediaType : unmanaged, Enum
		where TStatus : unmanaged, Enum
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
			? floorSurvivors.Where(result => EqualityComparer<TMediaType>.Default.Equals(result.Result.MediaType, mediaTypeFilter.Value))
			: floorSurvivors;
		var sorted = filtered
			.OrderBy(static result => result.Rank)
			.ThenByDescending(static result => result.Result.ListUserCount)
			.ThenBy(static result => result.Result.Id)
			.Select(static result => Adapt(result.Result, result.Rank))
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

		Add(result.AlternativeTitles?.Japanese, MatchRank.Native);
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

	private static SearchResult Adapt<TMediaType, TStatus>(BaseSearchResult<TMediaType, TStatus> result, MatchRank rank)
		where TMediaType : unmanaged, Enum
		where TStatus : unmanaged, Enum => new(
			result.Id,
			result.PrimaryTitle,
			rank,
			CreateOptionDescription(result),
			context => SearchEmbedBuilder.Build(result, context.RequesterDisplayName, context.RequesterAvatarUrl));

	private static string CreateOptionDescription<TMediaType, TStatus>(BaseSearchResult<TMediaType, TStatus> result)
		where TMediaType : unmanaged, Enum
		where TStatus : unmanaged, Enum
	{
		var descriptionParts = new List<string>(4);
		var mediaType = result.MediaType.Humanize(LetterCasing.Sentence);
		if (!string.IsNullOrWhiteSpace(mediaType))
		{
			descriptionParts.Add(mediaType);
		}

		if (result.StartDate?.Year is { } year)
		{
			descriptionParts.Add(year.ToString(CultureInfo.InvariantCulture));
		}

		if (result.Mean is { } mean)
		{
			descriptionParts.Add($"★ {mean.ToString("0.##", CultureInfo.InvariantCulture)}");
		}

		descriptionParts.Add($"{FormatMemberCount(result.ListUserCount)} members");
		return string.Join(" · ", descriptionParts);
	}

	private static string FormatMemberCount(uint memberCount) => memberCount switch
	{
		>= Million => $"{(memberCount / (double)Million).ToString("0.#", CultureInfo.InvariantCulture)}M",
		>= Thousand => $"{(memberCount / (double)Thousand).ToString("0.#", CultureInfo.InvariantCulture)}K",
		_ => memberCount.ToString(CultureInfo.InvariantCulture),
	};
}
