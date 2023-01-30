// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using System.Text.Json.Serialization;
using PaperMalKing.Common.Json;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official;

public sealed class Genre
{
	[JsonPropertyName("name")]
	[JsonConverter(typeof(StringPoolingJsonConverter))]
	public required string Name { get; init; }
}