// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Media;

public abstract class BaseMedia : IMultiLanguageName
{
	[JsonPropertyName("id")]
	public ulong Id { get; init; }

	[JsonPropertyName("name")]
	public string? Name { get; init; }

	[JsonPropertyName("russian")]
	public string? RussianName { get; init; }

	[JsonPropertyName("kind")]
	public string? Kind { get; init; }

	[JsonPropertyName("status")]
	public string? Status { get; init; }

	[JsonPropertyName("score")]
	public float? Score { get; init; }

	[JsonPropertyName("url")]
	[field: MaybeNull]
	public string Url
	{
		get;
		init => field = value.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? value : Constants.BaseUrl + value;
	}

	[JsonPropertyName("genres")]
	public IReadOnlyList<Genre> Genres { get; init; } = [];

	[JsonPropertyName("description")]
	public string? Description { get; init; }

	[JsonPropertyName("personRoles")]
	public IReadOnlyList<Role> PersonRoles { get; init; } = [];

	[JsonPropertyName("poster")]
	public MediaPoster? Poster { get; init; }

	protected abstract string Type { get; }
}
