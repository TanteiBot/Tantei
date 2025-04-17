// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2025 N0D4N

using System.Text.Json.Serialization;

namespace PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Media;

public abstract class BaseMedia
{
	[JsonPropertyName("genres")]
	public IReadOnlyList<Genre> Genres { get; init; } = [];

	[JsonPropertyName("description")]
	public string? Description { get; init; }

	[JsonPropertyName("personRoles")]
	public IReadOnlyList<Role> PersonRoles { get; init; } = [];

	protected abstract string Type { get; }
}