// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PaperMalKing.Shikimori.Wrapper.Models.Media;

internal abstract class BaseMedia
{
	[JsonPropertyName("id")]
	public uint Id { get; init; }

	[JsonPropertyName("name")]
	public string? Name { get; init; }

	[JsonPropertyName("russian")]
	public string? RussianName { get; init; }

	public string Image => Utils.GetImageUrl(this.Type, this.Id);

	[JsonPropertyName("kind")]
	public required string Kind { get; init; }

	[JsonPropertyName("kind")]
	public required string Status { get; init; }

	[JsonPropertyName("genres")]
	public required IReadOnlyList<Genre> Genres { get; init; }

	[JsonPropertyName("description")]
	public string? Description { get; init; }

	protected abstract string Type { get; }
}