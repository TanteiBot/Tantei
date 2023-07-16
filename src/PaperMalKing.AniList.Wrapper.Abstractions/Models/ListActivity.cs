// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Abstractions.Models;

public sealed class ListActivity
{
	[JsonPropertyName("status")]
	public required string Status { get; init; }

	[JsonPropertyName("progress")]
	public string? Progress { get; init; }

	[JsonPropertyName("createdAt")]
	public long CreatedAtTimestamp { get; init; }

	[JsonPropertyName("media")]
	public required Media Media { get; init; }
}