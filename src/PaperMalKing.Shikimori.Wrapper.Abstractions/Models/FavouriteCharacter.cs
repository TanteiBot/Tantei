// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Media;

namespace PaperMalKing.Shikimori.Wrapper.Abstractions.Models;

public sealed class FavouriteCharacter : IMultiLanguageName
{
	[JsonPropertyName("id")]
	public uint Id { get; init; }

	[JsonPropertyName("name")]
	public string? Name { get; init; }

	[JsonPropertyName("russian")]
	public string? RussianName { get; init; }

	[JsonPropertyName("description")]
	public string? Description { get; init; }

	[JsonPropertyName("poster")]
	public MediaPoster? Poster { get; init; }

	[JsonPropertyName("url")]
	[field: MaybeNull]
	public string Url
	{
		get;
		init => field = value.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? value : Constants.BaseUrl + value;
	}
}
