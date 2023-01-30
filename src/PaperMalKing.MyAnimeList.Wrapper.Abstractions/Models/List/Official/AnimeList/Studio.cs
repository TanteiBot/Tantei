// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using System.Text.Json.Serialization;
using PaperMalKing.Common.Json;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.AnimeList;

public sealed class Studio
{
	private string? _url;

	[JsonPropertyName("id")]
	[JsonRequired]
	public uint Id { get; internal set; }

	[JsonPropertyName("name")]
	[JsonConverter(typeof(StringPoolingJsonConverter))]
	[JsonRequired]
	public string Name { get; internal set; } = null!;

	public string Url => this._url ??= $"{Constants.BASE_URL}/anime/producer/{this.Id}";
}