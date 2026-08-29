// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Text.Json.Serialization;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Search;

public sealed class AnimeStartSeason
{
	[JsonPropertyName("year")]
	public required uint Year { get; init; }

	[JsonPropertyName("season")]
	public required AnimeSeason Season { get; init; }
}
