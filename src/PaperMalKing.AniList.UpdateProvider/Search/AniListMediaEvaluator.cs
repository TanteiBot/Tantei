// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;
using PaperMalKing.UpdatesProviders.Base.Search;

namespace PaperMalKing.AniList.UpdateProvider.Search;

internal static class AniListMediaEvaluator
{
	public static SearchEvaluation Evaluate(
		MatchKey queryKey,
		TitleLanguage titleLanguage,
		IEnumerable<AniListMediaCandidate> mediaCandidates)
	{
		ArgumentNullException.ThrowIfNull(queryKey);
		ArgumentNullException.ThrowIfNull(mediaCandidates);
		if (queryKey.IsEmpty)
		{
			throw new ArgumentException("The query Match Key cannot be empty.", nameof(queryKey));
		}

		var floorSurvivors = mediaCandidates
			.Select(candidate =>
			{
				var resolvedTitle = candidate.Title.GetTitle(titleLanguage);
				return (Candidate: candidate, ResolvedTitle: resolvedTitle, Rank: GetMatchRank(queryKey, candidate, resolvedTitle));
			})
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

		var sorted = floorSurvivors
			.OrderBy(static candidate => candidate.Rank)
			.ThenByDescending(static candidate => candidate.Candidate.Popularity)
			.ThenBy(static candidate => candidate.Candidate.Id)
			.Select(static candidate => Adapt(candidate.Candidate, candidate.ResolvedTitle, candidate.Rank))
			.ToArray();
		var rankedResults = Array.AsReadOnly(sorted);

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

	private static MatchRank GetMatchRank(MatchKey queryKey, AniListMediaCandidate candidate, string resolvedTitle)
	{
		var candidateKeys = CreateCandidateKeys(candidate, resolvedTitle);
		var exactMatch = candidateKeys.Find(candidateKey => candidateKey.Key.Equals(queryKey));
		if (exactMatch != default)
		{
			return exactMatch.Rank;
		}

		return candidateKeys.Exists(candidateKey => candidateKey.Key.Contains(queryKey)) ? MatchRank.Contains : MatchRank.None;
	}

	private static List<(MatchKey Key, MatchRank Rank)> CreateCandidateKeys(AniListMediaCandidate candidate, string resolvedTitle)
	{
		var candidateKeys = new List<(MatchKey Key, MatchRank Rank)>();
		var keys = new HashSet<MatchKey>();
		Add(resolvedTitle, MatchRank.Primary);
		Add(candidate.Title.Romaji, MatchRank.Primary);
		foreach (var synonym in candidate.Synonyms)
		{
			Add(synonym, MatchRank.Synonym);
		}

		Add(candidate.Title.Native, MatchRank.Native);
		Add(candidate.Title.English, MatchRank.English);
		return candidateKeys;

		void Add(string? title, MatchRank exactRank)
		{
			if (title is null)
			{
				return;
			}

			var key = MatchKey.Create(title);
			if (!key.IsEmpty && keys.Add(key))
			{
				candidateKeys.Add((key, exactRank));
			}
		}
	}

	private static SearchResult Adapt(AniListMediaCandidate candidate, string resolvedTitle, MatchRank rank) => new(
		candidate.Id,
		resolvedTitle,
		rank,
		candidate.OptionDescription,
		candidate.BuildEmbed);
}
