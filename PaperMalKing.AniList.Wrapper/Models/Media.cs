// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using PaperMalKing.AniList.Wrapper.Models.Enums;
using PaperMalKing.AniList.Wrapper.Models.Interfaces;

namespace PaperMalKing.AniList.Wrapper.Models;
#pragma warning disable CA1724
public sealed class Media : IImageble, ISiteUrlable, IIdentifiable
#pragma warning restore CA1724
{
	[JsonPropertyName("id")]
	public ulong Id { get; init; }

	[JsonPropertyName("title")]
	public required MediaTitle Title { get; init; }

	[JsonPropertyName("type")]
	public ListType Type { get; init; }

	[JsonPropertyName("siteUrl")]
	public required string Url { get; init; }

	[JsonPropertyName("format")]
	public MediaFormat? Format { get; init; }

	[JsonPropertyName("countryOfOrigin")]
	public required string CountryOfOrigin { get; init; }

	[JsonPropertyName("status")]
	public MediaStatus Status { get; init; }

	[JsonPropertyName("episodes")]
	public ushort? Episodes { get; init; }

	[JsonPropertyName("chapters")]
	public ushort? Chapters { get; init; }

	[JsonPropertyName("volumes")]
	public ushort? Volumes { get; init; }

	[JsonPropertyName("image")]
	public required Image Image { get; init; }

	[JsonPropertyName("description")]
	public required string Description { get; init; }

	[JsonPropertyName("genres")]
	public IReadOnlyList<string> Genres { get; init; } = Array.Empty<string>();

	[JsonPropertyName("tags")]
	public IReadOnlyList<MediaTag> Tags { get; init; } = Array.Empty<MediaTag>();

	[JsonPropertyName("studios")]
	public Connection<Studio> Studios { get; init; } = Connection<Studio>.Empty;

	[JsonPropertyName("staff")]
	public Connection<StaffEdge> Staff { get; init; } = Connection<StaffEdge>.Empty;
}