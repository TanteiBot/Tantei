// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace PaperMalKing.Shikimori.Wrapper.Models.Media;

internal sealed class Role
{
	[JsonPropertyName("roles")]
	public required string Name { get; init; }

	[JsonPropertyName("roles_russian")]
	public required string RussianName { get; init; }

	[JsonPropertyName("character")]
	public CharacterMini? Character { get; init; }

	[JsonPropertyName("person")]
	public Person? Person { get; init; }
}

[SuppressMessage("Design", "MA0048:File name must match type name")]
internal sealed class CharacterMini
{
	[JsonPropertyName("id")]
	public uint Id { get; init; }
}