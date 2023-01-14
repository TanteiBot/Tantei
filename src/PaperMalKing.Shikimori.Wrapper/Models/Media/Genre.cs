// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.Text.Json.Serialization;

namespace PaperMalKing.Shikimori.Wrapper.Models.Media;

internal sealed class Genre : IMultiLanguageName
{
	[JsonPropertyName("name")]
	public required string Name { get; init; }

	[JsonPropertyName("russian")]
	public required string RussianName { get; init; }
}