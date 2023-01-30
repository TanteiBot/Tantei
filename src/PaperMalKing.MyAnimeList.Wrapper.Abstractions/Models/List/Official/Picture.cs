// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using System.Text.Json.Serialization;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official;

public sealed class Picture
{
	[JsonPropertyName("large")]
	public string? Large { get; internal set; }

	[JsonPropertyName("medium")]
	[JsonRequired]
	public string Medium { get; internal set; } = null!;
}