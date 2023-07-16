// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.Text.Json.Serialization;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Interfaces;

namespace PaperMalKing.AniList.Wrapper.Abstractions.Models;

public sealed class Character : IImageble, ISiteUrlable, IIdentifiable
{
	[JsonPropertyName("name")]
	public required GenericName Name { get; init; }

	[JsonPropertyName("siteUrl")]
	public required string Url { get; init; }

	[JsonPropertyName("image")]
	public Image? Image { get; init; }

	[JsonPropertyName("media")]
	public required Page<Media> Media { get; init; }

	[JsonPropertyName("id")]
	public uint Id { get; init; }
}