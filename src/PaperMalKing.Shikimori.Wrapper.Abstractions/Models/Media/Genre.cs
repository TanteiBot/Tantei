// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.Text.Json.Serialization;
using PaperMalKing.Common.Json;

namespace PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Media;

public sealed class Genre : IMultiLanguageName
{
	[JsonPropertyName("name")]
	[JsonConverter(typeof(StringPoolingJsonConverter))]
	public required string Name { get; init; }

	[JsonPropertyName("russian")]
	[JsonConverter(typeof(StringPoolingJsonConverter))]
	public required string RussianName { get; init; }
}