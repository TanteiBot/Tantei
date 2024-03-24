// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.Text.Json.Serialization;
using PaperMalKing.Common.Json;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.MangaList;

public sealed class Author
{
	[JsonPropertyName("role")]
	[JsonConverter(typeof(ClearableStringPoolingJsonConverter))]
	public required string Role { get; init; }

	[JsonPropertyName("node")]
	public required Person Person { get; init; }
}