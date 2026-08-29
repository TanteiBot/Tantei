// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Collections.ObjectModel;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Search;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Search;

internal static class SearchResultRanker
{
	public static ReadOnlyCollection<TitleMatchCandidate> CreateCandidateKeys<TMediaType, TStatus>(BaseSearchResult<TMediaType, TStatus> result)
		where TMediaType : unmanaged, Enum
		where TStatus : unmanaged, Enum
	{
		ArgumentNullException.ThrowIfNull(result);
		var candidates = new List<TitleMatchCandidate>();
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
		return candidates.AsReadOnly();

		void Add(string? title, MatchRank exactRank)
		{
			if (title is null)
			{
				return;
			}

			var key = MatchKey.Create(title);
			if (!key.IsEmpty && keys.Add(key))
			{
				candidates.Add(new(key, exactRank));
			}
		}
	}

	public static MatchRank GetMatchRank<TMediaType, TStatus>(MatchKey queryKey, BaseSearchResult<TMediaType, TStatus> result)
		where TMediaType : unmanaged, Enum
		where TStatus : unmanaged, Enum
	{
		ArgumentNullException.ThrowIfNull(queryKey);
		if (queryKey.IsEmpty)
		{
			throw new ArgumentException("The query Match Key cannot be empty.", nameof(queryKey));
		}

		var candidates = CreateCandidateKeys(result);
		var exactMatch = candidates.FirstOrDefault(candidate => candidate.Key.Equals(queryKey));
		if (exactMatch is not null)
		{
			return exactMatch.ExactRank;
		}

		return candidates.Any(candidate => candidate.Key.Contains(queryKey)) ? MatchRank.Contains : MatchRank.None;
	}
}
