// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Globalization;
using Humanizer;
using PaperMalKing.AniList.Wrapper.Abstractions.Models;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;

namespace PaperMalKing.AniList.UpdateProvider.Search;

internal static class AniListSearchPresentation
{
	private const uint Thousand = 1_000U;
	private const uint Million = 1_000_000U;
	private const string AdultBadge = "🔞";

	public static string BuildOptionDescription(SearchMedia media, ScoreFormat scoreFormat)
	{
		ArgumentNullException.ThrowIfNull(media);
		var parts = new List<string>(5);
		if (media.Format is { } format)
		{
			var humanizedFormat = format.Humanize(LetterCasing.Sentence);
			if (!string.IsNullOrWhiteSpace(humanizedFormat))
			{
				parts.Add(humanizedFormat);
			}
		}

		if (media.SeasonYear is { } year)
		{
			parts.Add(year.ToString(CultureInfo.InvariantCulture));
		}

		var score = AniListScoreFormatter.Format(media.AverageScore, scoreFormat);
		if (score is not null)
		{
			parts.Add(score);
		}

		parts.Add(FormatPopularity(media.Popularity));
		if (media.IsAdult)
		{
			parts.Add(AdultBadge);
		}

		return string.Join(" · ", parts);
	}

	public static string FormatPopularity(uint popularity) => popularity switch
	{
		>= Million => $"{(popularity / (double)Million).ToString("0.#", CultureInfo.InvariantCulture)}M",
		>= Thousand => $"{(popularity / (double)Thousand).ToString("0.#", CultureInfo.InvariantCulture)}K",
		_ => popularity.ToString(CultureInfo.InvariantCulture),
	};
}
