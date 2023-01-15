// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.Text.Json.Serialization;
using PaperMalKing.Common.Json;

namespace PaperMalKing.Shikimori.Wrapper.Models.Media;

internal sealed class Studio
{
	[JsonPropertyName("id")]
	public required uint Id { get; init; }

	[JsonPropertyName("name")]
	[JsonConverter(typeof(StringPoolingJsonConverter))]
	public required string Name { get; init; }

	public string Url => Utils.GetUrl("animes/studio", this.Id);

	public string Image => Utils.GetImageUrl("studios", this.Id);
}