// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System.Text.Json.Serialization;
using PaperMalKing.AniList.Wrapper.Models.Interfaces;

namespace PaperMalKing.AniList.Wrapper.Models;

public sealed class User : ISiteUrlable, IImageble
{
	[JsonPropertyName("id")]
	public ulong Id { get; init; }

	[JsonPropertyName("name")]
	public required string Name { get; init; }

	[JsonPropertyName("siteUrl")]
	public required string Url { get; init; }

	[JsonPropertyName("image")]
	public required Image Image { get; init; }

	[JsonPropertyName("options")]
	public required UserOptions Options { get; init; }

	[JsonPropertyName("mediaListOptions")]
	public required MediaListOptions MediaListOptions { get; init; }

	[JsonPropertyName("favourites")]
	public required Favourites Favourites { get; init; }
}