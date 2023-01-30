// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.Collections.Generic;
using System.Text.Json.Serialization;
using PaperMalKing.Common.Json;

namespace PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Media;

public abstract class BaseMedia : IMultiLanguageName
{
	[JsonPropertyName("id")]
	public uint Id { get; init; }

	[JsonPropertyName("name")]
	public string? Name { get; init; }

	[JsonPropertyName("russian")]
	public string? RussianName { get; init; }

	public string Image => Utils.GetImageUrl(this.Type, this.Id);

	[JsonPropertyName("kind")]
	[JsonConverter(typeof(StringPoolingJsonConverter))]
	public required string Kind { get; init; }

	[JsonPropertyName("status")]
	[JsonConverter(typeof(StringPoolingJsonConverter))]
	public required string Status { get; init; }

	[JsonPropertyName("genres")]
	public required IReadOnlyList<Genre> Genres { get; init; }

	[JsonPropertyName("description")]
	public required string Description { get; init; }

	protected abstract string Type { get; }
}