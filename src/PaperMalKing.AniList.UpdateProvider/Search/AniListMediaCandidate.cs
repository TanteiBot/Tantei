// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using DSharpPlus.Entities;
using PaperMalKing.AniList.Wrapper.Abstractions.Models;
using PaperMalKing.UpdatesProviders.Base.Search;

namespace PaperMalKing.AniList.UpdateProvider.Search;

internal sealed record AniListMediaCandidate(
	uint Id,
	MediaTitle Title,
	IReadOnlyList<string?> Synonyms,
	int Popularity,
	string OptionDescription,
	Func<PickerSearchContext, DiscordEmbedBuilder> BuildEmbed);
