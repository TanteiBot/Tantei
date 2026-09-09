// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Text.Json.Serialization;

namespace PaperMalKing.Shikimori.Wrapper.Abstractions.Models;

public sealed class RelatedCharacter : IMultiLanguageName
{
	[JsonPropertyName("id")]
	public uint Id { get; init; }

	[JsonPropertyName("name")]
	public string? Name { get; init; }

	[JsonPropertyName("russian")]
	public string? RussianName { get; init; }
}
