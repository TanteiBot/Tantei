// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Text.Json.Serialization;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.AnimeList;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Search;

public sealed class AnimeSearchResult : BaseSearchResult<AnimeMediaType, AnimeAiringStatus>
{
	[JsonPropertyName("num_episodes")]
	public required uint Episodes { get; init; }

	[JsonPropertyName("start_season")]
	public AnimeStartSeason? StartSeason { get; init; }
}
