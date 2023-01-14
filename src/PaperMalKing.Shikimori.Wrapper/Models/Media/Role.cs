// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace PaperMalKing.Shikimori.Wrapper.Models.Media;

internal sealed class Role
{
	[JsonPropertyName("roles")]
	public required IReadOnlyList<string> Name { get; init; }

	[JsonPropertyName("roles_russian")]
	public required IReadOnlyList<string> RussianName { get; init; }

	[JsonPropertyName("person")]
	public Person? Person { get; init; }
}