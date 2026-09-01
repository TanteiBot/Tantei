// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Text.Json.Serialization;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Search;

public sealed class AlternativeTitles
{
	[JsonPropertyName("synonyms")]
	public IReadOnlyList<string?>? Synonyms { get; init; }

	[JsonPropertyName("en")]
	public string? English { get; init; }

	[JsonPropertyName("ja")]
	public string? Japanese { get; init; }
}
