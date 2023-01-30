// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using System.Text.Json.Serialization;
using PaperMalKing.Common.Json;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.MangaList;

public sealed class Person
{
	private string? _url;
	[JsonPropertyName("id")]
	[JsonRequired]
	public uint Id { get; internal set; }

	[JsonPropertyName("first_name")]
	[JsonConverter(typeof(ClearableStringPoolingJsonConverter))]
	public string? FirstName { get; internal set; }

	[JsonPropertyName("last_name")]
	[JsonConverter(typeof(ClearableStringPoolingJsonConverter))]
	public string? LastName { get; internal set; }

	public string Url => this._url ??= $"{Constants.BASE_URL}/people/{this.Id}";
}