// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Text.Json.Serialization;

namespace PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Media;

public sealed class MangaSearchMedia : MangaMedia, ISearchMedia
{
	[JsonPropertyName("english")]
	public string? EnglishName { get; init; }

	[JsonPropertyName("japanese")]
	public string? JapaneseName { get; init; }

	[JsonPropertyName("synonyms")]
	public IReadOnlyList<string> Synonyms { get; init; } = [];

	[JsonPropertyName("airedOn")]
	public IncompleteDate? AiredOn { get; init; }

	[JsonPropertyName("statusesStats")]
	public IReadOnlyList<StatusStat> StatusesStats { get; init; } = [];

	[JsonIgnore]
	public int? Year => this.AiredOn?.Year;

	[JsonIgnore]
	public long Popularity => this.StatusesStats.Sum(static stat => stat.Count);

	[JsonIgnore]
	public bool IsAdult => false;
}
