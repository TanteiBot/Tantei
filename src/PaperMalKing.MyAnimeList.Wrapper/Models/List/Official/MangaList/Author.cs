// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using System.Text.Json.Serialization;
using PaperMalKing.Common.Json;

namespace PaperMalKing.MyAnimeList.Wrapper.Models.List.Official.MangaList;

internal sealed class Author
{
	[JsonPropertyName("role")]
	[JsonConverter(typeof(ClearableStringPoolingJsonConverter))]
	public required string Role { get; init; }

	[JsonPropertyName("node")]
	public required Person Person { get; init; }
}