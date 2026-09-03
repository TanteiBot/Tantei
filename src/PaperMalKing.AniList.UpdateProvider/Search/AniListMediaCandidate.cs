// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using DSharpPlus.Entities;
using PaperMalKing.AniList.Wrapper.Abstractions.Models;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;
using PaperMalKing.UpdatesProviders.Base.Search;

namespace PaperMalKing.AniList.UpdateProvider.Search;

internal static class AniListMediaCandidate
{
	public static SearchCandidate Create(
		uint id,
		MediaTitle title,
		IReadOnlyList<string?> synonyms,
		long popularity,
		TitleLanguage titleLanguage,
		string optionDescription,
		Func<PickerSearchContext, DiscordEmbedBuilder> buildEmbed)
	{
		ArgumentNullException.ThrowIfNull(title);
		ArgumentNullException.ThrowIfNull(synonyms);
		var resolvedTitle = title.GetTitle(titleLanguage);
		var matchTitles = new List<(string? Title, MatchRank Rank)>(synonyms.Count + 4)
		{
			(resolvedTitle, MatchRank.Primary),
			(title.Romaji, MatchRank.Primary),
		};
		foreach (var synonym in synonyms)
		{
			matchTitles.Add((synonym, MatchRank.Synonym));
		}

		matchTitles.Add((title.Native, MatchRank.Native));
		matchTitles.Add((title.English, MatchRank.English));
		return new(id, popularity, resolvedTitle, matchTitles, optionDescription, buildEmbed);
	}
}
