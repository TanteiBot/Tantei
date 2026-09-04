// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Globalization;
using Humanizer;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Search;
using PaperMalKing.UpdatesProviders.Base.Search;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Search;

internal static class MalMediaCandidate
{
	public static SearchCandidate Create<TMediaType, TStatus>(BaseSearchResult<TMediaType, TStatus> result, TMediaType? mediaTypeFilter)
		where TMediaType : unmanaged, Enum
		where TStatus : unmanaged, Enum
	{
		ArgumentNullException.ThrowIfNull(result);
		var matchTitles = new List<(string? Title, MatchRank Rank)>((result.AlternativeTitles?.Synonyms?.Count ?? 0) + 3)
		{
			(result.PrimaryTitle, MatchRank.Primary),
		};
		if (result.AlternativeTitles?.Synonyms is { } synonyms)
		{
			foreach (var synonym in synonyms)
			{
				matchTitles.Add((synonym, MatchRank.Synonym));
			}
		}

		matchTitles.Add((result.AlternativeTitles?.Japanese, MatchRank.Native));
		matchTitles.Add((result.AlternativeTitles?.English, MatchRank.English));

		var passesTypeFilter = !mediaTypeFilter.HasValue
			|| EqualityComparer<TMediaType>.Default.Equals(result.MediaType, mediaTypeFilter.Value);
		return new(
			result.Id,
			result.ListUserCount,
			result.PrimaryTitle,
			matchTitles,
			DescribeOption(result),
			context => SearchEmbedBuilder.Build(result, context.RequesterDisplayName, context.RequesterAvatarUrl),
			passesTypeFilter);
	}

	private static string DescribeOption<TMediaType, TStatus>(BaseSearchResult<TMediaType, TStatus> result)
		where TMediaType : unmanaged, Enum
		where TStatus : unmanaged, Enum
	{
		var mediaType = result.MediaType.Humanize(LetterCasing.Sentence);
		var year = result.StartDate?.Year is { } startYear ? startYear.ToString(CultureInfo.InvariantCulture) : null;
		var score = result.Mean is { } mean ? $"★ {mean.ToString("0.##", CultureInfo.InvariantCulture)}" : null;
		var members = $"{SearchPresentation.AbbreviateCount(result.ListUserCount)} members";
		return SearchPresentation.ComposeOptionDescription([mediaType, year, score, members]);
	}
}
