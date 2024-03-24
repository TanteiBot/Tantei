// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Abstractions.Models;

public sealed class CustomList
{
	[JsonPropertyName("name")]
	public required string Name { get; init; }

	[JsonPropertyName("enabled")]
	public bool Enabled { get; init; }
}