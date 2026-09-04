// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Globalization;
using DSharpPlus.Entities;
using Humanizer;
using PaperMalKing.AniList.Wrapper.Abstractions.Models;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;
using PaperMalKing.UpdatesProviders.Base.Search;

namespace PaperMalKing.AniList.UpdateProvider.Search;

internal static class AniListMediaCandidate
{
	private const string AdultBadge = "🔞";

	public static SearchCandidate Create(
		SearchMedia media,
		TitleLanguage titleLanguage,
		ScoreFormat scoreFormat,
		Func<PickerSearchContext, DiscordEmbedBuilder> buildEmbed)
	{
		ArgumentNullException.ThrowIfNull(media);
		ArgumentNullException.ThrowIfNull(buildEmbed);
		var resolvedTitle = media.Title.GetTitle(titleLanguage);
		var matchTitles = new List<(string? Title, MatchRank Rank)>(media.Synonyms.Count + 4)
		{
			(resolvedTitle, MatchRank.Primary),
			(media.Title.Romaji, MatchRank.Primary),
		};
		foreach (var synonym in media.Synonyms)
		{
			matchTitles.Add((synonym, MatchRank.Synonym));
		}

		matchTitles.Add((media.Title.Native, MatchRank.Native));
		matchTitles.Add((media.Title.English, MatchRank.English));
		return new(media.Id, media.Popularity, resolvedTitle, matchTitles, DescribeOption(media, scoreFormat), buildEmbed);
	}

	private static string DescribeOption(SearchMedia media, ScoreFormat scoreFormat)
	{
		var format = media.Format is { } mediaFormat ? mediaFormat.Humanize(LetterCasing.Sentence) : null;
		var year = media.SeasonYear is { } seasonYear ? seasonYear.ToString(CultureInfo.InvariantCulture) : null;
		var score = AniListScoreFormatter.Format(media.AverageScore, scoreFormat);
		var popularity = SearchPresentation.AbbreviateCount(media.Popularity);
		var badge = media.IsAdult ? AdultBadge : null;
		return SearchPresentation.ComposeOptionDescription([format, year, score, popularity, badge]);
	}
}
