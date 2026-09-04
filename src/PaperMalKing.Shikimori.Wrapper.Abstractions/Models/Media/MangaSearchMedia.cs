// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Media;

public sealed class MangaSearchMedia : MangaMedia, ISearchMedia
{
	[JsonPropertyName("id")]
	public ulong Id { get; init; }

	[JsonPropertyName("name")]
	public required string Name { get; init; }

	[JsonPropertyName("russian")]
	public string? RussianName { get; init; }

	[JsonPropertyName("english")]
	public string? EnglishName { get; init; }

	[JsonPropertyName("japanese")]
	public string? JapaneseName { get; init; }

	[JsonPropertyName("synonyms")]
	public IReadOnlyList<string> Synonyms { get; init; } = [];

	[JsonPropertyName("kind")]
	public string? Kind { get; init; }

	[JsonPropertyName("score")]
	public float? Score { get; init; }

	[JsonPropertyName("status")]
	public string? Status { get; init; }

	[JsonPropertyName("airedOn")]
	public IncompleteDate? AiredOn { get; init; }

	[JsonPropertyName("statusesStats")]
	public IReadOnlyList<StatusStat> StatusesStats { get; init; } = [];

	[JsonPropertyName("url")]
	[field: MaybeNull]
	public string Url
	{
		get;
		init => field = value.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? value : Constants.BaseUrl + value;
	}

	[JsonIgnore]
	public int? Year => this.AiredOn?.Year;

	[JsonIgnore]
	public long Popularity => this.StatusesStats.Sum(static stat => stat.Count);

	[JsonIgnore]
	public bool IsAdult => false;
}
