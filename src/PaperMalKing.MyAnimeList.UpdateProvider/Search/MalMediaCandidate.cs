// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Globalization;
using Humanizer;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Search;
using PaperMalKing.UpdatesProviders.Base.Search;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Search;

internal static class MalMediaCandidate
{
	private const uint Thousand = 1_000U;
	private const uint Million = 1_000_000U;

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
			CreateOptionDescription(result),
			context => SearchEmbedBuilder.Build(result, context.RequesterDisplayName, context.RequesterAvatarUrl),
			passesTypeFilter);
	}

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
