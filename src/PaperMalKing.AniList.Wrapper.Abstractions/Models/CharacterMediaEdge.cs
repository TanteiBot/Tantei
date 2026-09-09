// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Abstractions.Models;

public sealed class CharacterMediaEdge
{
	[JsonPropertyName("characters")]
	public IReadOnlyList<VoicedCharacter>? Characters { get; init; }

	[JsonPropertyName("node")]
	public Media? Node { get; init; }
}

[SuppressMessage("Design", "MA0048:File name must match type name", Justification = "Parts of one payload")]
public sealed class VoicedCharacter
{
	[JsonPropertyName("name")]
	public required GenericName Name { get; init; }
}
