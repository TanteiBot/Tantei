// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using System.Text.Json.Serialization;

namespace PaperMalKing.MyAnimeList.Wrapper.Models.List.Official;

public sealed class Genre
{
	[JsonPropertyName("name")]
	public required string Name { get; init; }
}