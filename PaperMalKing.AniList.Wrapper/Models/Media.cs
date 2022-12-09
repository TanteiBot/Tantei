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
	public MediaTitle Title { get; init; } = null!;

	[JsonPropertyName("type")]
	public ListType Type { get; init; }

	[JsonPropertyName("siteUrl")]
	public string Url { get; init; } = null!;

	[JsonPropertyName("format")]
	public MediaFormat? Format { get; init; }

	[JsonPropertyName("countryOfOrigin")]
	public string CountryOfOrigin { get; init; } = null!;

	[JsonPropertyName("status")]
	public MediaStatus Status { get; init; }

	[JsonPropertyName("episodes")]
	public ushort? Episodes { get; init; }

	[JsonPropertyName("chapters")]
	public ushort? Chapters { get; init; }

	[JsonPropertyName("volumes")]
	public ushort? Volumes { get; init; }

	[JsonPropertyName("image")]
	public Image Image { get; init; } = null!;

	[JsonPropertyName("description")]
	public string Description { get; init; } = null!;

	[JsonPropertyName("genres")]
	public IReadOnlyList<string> Genres { get; init; } = Array.Empty<string>();

	[JsonPropertyName("tags")]
	public IReadOnlyList<MediaTag> Tags { get; init; } = Array.Empty<MediaTag>();

	[JsonPropertyName("studios")]
	public Connection<Studio> Studios { get; init; } = Connection<Studio>.Empty;

	[JsonPropertyName("staff")]
	public Connection<StaffEdge> Staff { get; init; } = Connection<StaffEdge>.Empty;
}