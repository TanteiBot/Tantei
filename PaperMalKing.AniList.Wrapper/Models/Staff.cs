// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using PaperMalKing.AniList.Wrapper.Models.Interfaces;

namespace PaperMalKing.AniList.Wrapper.Models;

public sealed class Staff : IImageble, ISiteUrlable, IIdentifiable
{
	[JsonPropertyName("name")]
	public GenericName Name { get; init; } = null!;

	[JsonPropertyName("siteUrl")]
	public string Url { get; init; } = null!;

	[JsonPropertyName("image")]
	public Image Image { get; init; } = null!;

	[JsonPropertyName("id")]
	public ulong Id { get; init; }

	[JsonPropertyName("description")]
	public string Description { get; init; } = null!;

	[JsonPropertyName("staffMedia")]
	public Connection<Media> StaffMedia { get; init; } = Connection<Media>.Empty;

	[JsonPropertyName("primaryOccupations")]
	public IReadOnlyList<string> PrimaryOccupations { get; init; } = Array.Empty<string>();
}