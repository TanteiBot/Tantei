// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.Text.Json.Serialization;
using PaperMalKing.Common.Json;

namespace PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Media;

public sealed class Studio
{
	[JsonPropertyName("id")]
	[JsonRequired]
	public uint Id { get; internal set; }

	[JsonPropertyName("name")]
	[JsonConverter(typeof(StringPoolingJsonConverter))]
	[JsonRequired]
	public string Name { get; internal set; } = null!;

	public string Url => Utils.GetUrl("animes/studio", this.Id);

	public string Image => Utils.GetImageUrl("studios", this.Id);
}