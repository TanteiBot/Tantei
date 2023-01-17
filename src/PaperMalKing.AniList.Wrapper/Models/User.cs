// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System.Text.Json.Serialization;
using PaperMalKing.AniList.Wrapper.Models.Interfaces;
using PaperMalKing.Common.Json;

namespace PaperMalKing.AniList.Wrapper.Models;

internal sealed class User : ISiteUrlable, IImageble
{
	[JsonPropertyName("id")]
	public uint Id { get; init; }

	[JsonPropertyName("name")]
	[JsonConverter(typeof(ClearableStringPoolingJsonConverter))]
	public string? Name { get; init; }

	[JsonPropertyName("siteUrl")]
	public required string Url { get; init; }

	[JsonPropertyName("image")]
	public Image? Image { get; init; }

	[JsonPropertyName("options")]
	public UserOptions Options { get; init; } = null!;

	[JsonPropertyName("mediaListOptions")]
	public MediaListOptions? MediaListOptions { get; init; }

	[JsonPropertyName("favourites")]
	public Favourites Favourites { get; init; } = Favourites.Empty;
}