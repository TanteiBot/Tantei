// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using System.Text.Json.Serialization;

namespace PaperMalKing.MyAnimeList.Wrapper.Models.List.Official.AnimeList;

internal sealed class Studio
{
	private string? _url;

	[JsonPropertyName("id")]
	public required uint Id { get; init; }

	[JsonPropertyName("name")]
	public required string Name { get; init; }

	public string Url => this._url ??= $"{Constants.BASE_URL}/anime/producer/{Id}";
}