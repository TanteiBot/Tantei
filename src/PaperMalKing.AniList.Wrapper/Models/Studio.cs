// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System.Text.Json.Serialization;
using PaperMalKing.AniList.Wrapper.Models.Interfaces;
using PaperMalKing.Common.Json;

namespace PaperMalKing.AniList.Wrapper.Models;

internal sealed class Studio : ISiteUrlable, IIdentifiable
{
	[JsonPropertyName("name")]
	[JsonConverter(typeof(StringPoolingJsonConverter))]
	public required string Name { get; init; }

	[JsonPropertyName("siteUrl")]
	public required string Url { get; init; }

	[JsonPropertyName("media")]
	public Connection<Media> Media { get; init; } = Connection<Media>.Empty;

	[JsonPropertyName("isAnimationStudio")]
	public bool IsAnimationStudio { get; init; }

	[JsonPropertyName("id")]
	public uint Id { get; init; }
}