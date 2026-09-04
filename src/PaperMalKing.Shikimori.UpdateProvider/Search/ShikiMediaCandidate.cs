// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Globalization;
using Humanizer;
using PaperMalKing.Common.Enums;
using PaperMalKing.Database.Models.Shikimori;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Media;
using PaperMalKing.UpdatesProviders.Base.Search;

namespace PaperMalKing.Shikimori.UpdateProvider.Search;

internal static class ShikiMediaCandidate
{
	private const string AdultBadge = "🔞";

	public static SearchCandidate Create<TMedia>(
		TMedia media,
		ListEntryType type,
		ShikiUserFeatures features,
		bool useRussian,
		string? requestedKindToken)
		where TMedia : BaseMedia, ISearchMedia
	{
		ArgumentNullException.ThrowIfNull(media);
		var resolvedTitle = media.GetNameOrAltName(useRussian);
		var matchTitles = new List<(string? Title, MatchRank Rank)>(media.Synonyms.Count + 4)
		{
			(resolvedTitle, MatchRank.Primary),
			(media.Name, MatchRank.Primary),
		};
		foreach (var synonym in media.Synonyms)
		{
			matchTitles.Add((synonym, MatchRank.Synonym));
		}

		matchTitles.Add((media.JapaneseName, MatchRank.Native));
		matchTitles.Add((media.EnglishName, MatchRank.English));

		var passesTypeFilter = requestedKindToken is null || string.Equals(media.Kind, requestedKindToken, StringComparison.OrdinalIgnoreCase);
		return new(
			(uint)media.Id,
			media.Popularity,
			resolvedTitle,
			matchTitles,
			DescribeOption(media),
			context => SearchEmbedBuilder.Build(media, type, features, useRussian, context.RequesterDisplayName, context.RequesterAvatarUrl),
			passesTypeFilter);
	}

	private static string DescribeOption(ISearchMedia media)
	{
		var kind = string.IsNullOrWhiteSpace(media.Kind) ? null : media.Kind.Humanize(LetterCasing.Sentence);
		var year = media.Year is { } releaseYear ? releaseYear.ToString(CultureInfo.InvariantCulture) : null;
		var score = media.Score is > 0f ? $"★ {media.Score.Value.ToString("0.##", CultureInfo.InvariantCulture)}" : null;
		var popularity = media.Popularity > 0 ? SearchPresentation.AbbreviateCount((uint)Math.Min(media.Popularity, uint.MaxValue)) : null;
		var badge = media.IsAdult ? AdultBadge : null;
		return SearchPresentation.ComposeOptionDescription([kind, year, score, popularity, badge]);
	}
}
