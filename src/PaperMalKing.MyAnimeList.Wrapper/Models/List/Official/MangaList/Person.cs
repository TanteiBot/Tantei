// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using System.Text.Json.Serialization;
using PaperMalKing.Common.Json;

namespace PaperMalKing.MyAnimeList.Wrapper.Models.List.Official.MangaList;

internal sealed class Person
{
	private string? _url;
	[JsonPropertyName("id")]
	public required uint Id { get; init; }

	[JsonPropertyName("first_name")]
	[JsonConverter(typeof(ClearableStringPoolingJsonConverter))]
	public string? FirstName { get; init; }

	[JsonPropertyName("last_name")]
	[JsonConverter(typeof(ClearableStringPoolingJsonConverter))]
	public string? LastName { get; init; }

	public string Url => this._url ??= $"{Constants.BASE_URL}/people/{Id}";
}