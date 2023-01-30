// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Interfaces;

namespace PaperMalKing.AniList.Wrapper.Abstractions.Models;

public sealed class Staff : IImageble, ISiteUrlable, IIdentifiable
{
	[JsonPropertyName("name")]
	public required GenericName Name { get; init; }

	[JsonPropertyName("siteUrl")]
	public required string Url { get; init; }

	[JsonPropertyName("image")]
	public Image? Image { get; init; }

	[JsonPropertyName("id")]
	public uint Id { get; init; }

	[JsonPropertyName("description")]
	public string? Description { get; init; } 

	[JsonPropertyName("staffMedia")]
	public Connection<Media> StaffMedia { get; init; } = Connection<Media>.Empty;

	[JsonPropertyName("primaryOccupations")]
	public IReadOnlyList<string> PrimaryOccupations { get; init; } = Array.Empty<string>();
}