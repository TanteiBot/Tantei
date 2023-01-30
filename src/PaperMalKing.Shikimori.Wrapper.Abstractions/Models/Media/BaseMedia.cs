// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.Collections.Generic;
using System.Text.Json.Serialization;
using PaperMalKing.Common.Json;

namespace PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Media;

public abstract class BaseMedia : IMultiLanguageName
{
	[JsonPropertyName("id")]
	public uint Id { get; internal set; }

	[JsonPropertyName("name")]
	public string? Name { get; internal set; }

	[JsonPropertyName("russian")]
	public string? RussianName { get; internal set; }

	public string Image => Utils.GetImageUrl(this.Type, this.Id);

	[JsonPropertyName("kind")]
	[JsonConverter(typeof(StringPoolingJsonConverter))]
	[JsonRequired]
	public string Kind { get; internal set; } = null!;

	[JsonPropertyName("status")]
	[JsonConverter(typeof(StringPoolingJsonConverter))]
	[JsonRequired]
	public string Status { get; internal set; } = null!;

	[JsonPropertyName("genres")]
	[JsonRequired]
	public IReadOnlyList<Genre> Genres { get; internal set; } = null!;

	[JsonPropertyName("description")]
	[JsonRequired]
	public string Description { get; internal set; } = null!;

	protected abstract string Type { get; }
}