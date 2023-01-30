// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using System.Text.Json.Serialization;
using PaperMalKing.Common.Json;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.MangaList;

public sealed class Author
{
	[JsonPropertyName("role")]
	[JsonConverter(typeof(ClearableStringPoolingJsonConverter))]
	[JsonRequired]
	public string Role { get; internal set; } = null!;

	[JsonPropertyName("node")]
	[JsonRequired]
	public Person Person { get; internal set; } = null!;
}