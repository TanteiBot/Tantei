// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using DSharpPlus.Entities;

namespace PaperMalKing.UpdatesProviders.Base.Search;

internal sealed record SearchCandidate(
	uint Id,
	long Popularity,
	string PrimaryTitle,
	IReadOnlyList<(string? Title, MatchRank Rank)> MatchTitles,
	string OptionDescription,
	Func<PickerSearchContext, DiscordEmbedBuilder> BuildEmbed,
	bool PassesTypeFilter = true);
