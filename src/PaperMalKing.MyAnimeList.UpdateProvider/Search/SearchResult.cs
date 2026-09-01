// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using DSharpPlus.Entities;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Search;

internal sealed class SearchResult(
	uint id,
	string primaryTitle,
	MatchRank rank,
	Func<PickerSearchContext, DiscordEmbedBuilder> buildEmbed)
{
	private readonly Func<PickerSearchContext, DiscordEmbedBuilder> _buildEmbed = buildEmbed;

	public MatchRank Rank { get; } = rank;

	public uint Id { get; } = id;

	public string PrimaryTitle { get; } = primaryTitle;

	public string MediaType { get; init; } = string.Empty;

	public int? Year { get; init; }

	public double? Mean { get; init; }

	public uint ListUserCount { get; init; }

	public DiscordEmbedBuilder BuildEmbed(PickerSearchContext context) => this._buildEmbed(context);
}
