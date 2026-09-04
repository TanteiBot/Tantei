// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

namespace PaperMalKing.UpdatesProviders.Base.Search;

internal static class SearchEvaluator
{
	public static SearchEvaluation Evaluate(MatchKey queryKey, IEnumerable<SearchCandidate> candidates, bool applyTypeFilter = false)
	{
		ArgumentNullException.ThrowIfNull(queryKey);
		ArgumentNullException.ThrowIfNull(candidates);
		if (queryKey.IsEmpty)
		{
			throw new ArgumentException("The query Match Key cannot be empty.", nameof(queryKey));
		}

		var floorSurvivors = candidates
			.Select(candidate => (Candidate: candidate, Rank: GetMatchRank(queryKey, candidate.MatchTitles)))
			.Where(static candidate => candidate.Rank != MatchRank.None)
			.ToArray();
		if (floorSurvivors.Length == 0)
		{
			return new(
				Kind: SearchOutcomeKind.NoResults,
				Results: [],
				FloorSurvivorCount: 0,
				AutoPostResult: null);
		}

		var filtered = applyTypeFilter
			? floorSurvivors.Where(static candidate => candidate.Candidate.PassesTypeFilter)
			: floorSurvivors;
		var sorted = filtered
			.OrderBy(static candidate => candidate.Rank)
			.ThenByDescending(static candidate => candidate.Candidate.Popularity)
			.ThenBy(static candidate => candidate.Candidate.Id)
			.Select(static candidate => Adapt(candidate.Candidate, candidate.Rank))
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

	private static MatchRank GetMatchRank(MatchKey queryKey, IReadOnlyList<(string? Title, MatchRank Rank)> matchTitles)
	{
		var candidateKeys = CreateCandidateKeys(matchTitles);
		var exactMatch = candidateKeys.Find(candidateKey => candidateKey.Key.Equals(queryKey));
		if (exactMatch != default)
		{
			return exactMatch.Rank;
		}

		return candidateKeys.Exists(candidateKey => candidateKey.Key.Contains(queryKey)) ? MatchRank.Contains : MatchRank.None;
	}

	private static List<(MatchKey Key, MatchRank Rank)> CreateCandidateKeys(IReadOnlyList<(string? Title, MatchRank Rank)> matchTitles)
	{
		var candidateKeys = new List<(MatchKey Key, MatchRank Rank)>(matchTitles.Count);
		var keys = new HashSet<MatchKey>();
		foreach (var (title, rank) in matchTitles)
		{
			if (title is null)
			{
				continue;
			}

			var key = MatchKey.Create(title);
			if (!key.IsEmpty && keys.Add(key))
			{
				candidateKeys.Add((key, rank));
			}
		}

		return candidateKeys;
	}

	private static SearchResult Adapt(SearchCandidate candidate, MatchRank rank) => new(
		candidate.Id,
		candidate.PrimaryTitle,
		rank,
		candidate.OptionDescription,
		candidate.BuildEmbed);
}
