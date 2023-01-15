// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using PaperMalKing.AniList.Wrapper.Models.Enums;
using PaperMalKing.AniList.Wrapper.Models.Interfaces;
using PaperMalKing.Common.Json;

namespace PaperMalKing.AniList.Wrapper.Models;
#pragma warning disable CA1724
internal sealed class Media : IImageble, ISiteUrlable, IIdentifiable
#pragma warning restore CA1724
{
	[JsonPropertyName("id")]
	public uint Id { get; init; }

	[JsonPropertyName("title")]
	public required MediaTitle Title { get; init; }

	[JsonPropertyName("type")]
	public ListType Type { get; init; }

	[JsonPropertyName("siteUrl")]
	public required string Url { get; init; }

	[JsonPropertyName("format")]
	public MediaFormat? Format { get; init; }

	[JsonPropertyName("countryOfOrigin")]
	[JsonConverter(typeof(StringPoolingJsonConverter))]
	public string? CountryOfOrigin { get; init; }

	[JsonPropertyName("status")]
	public MediaStatus Status { get; init; }

	[JsonPropertyName("episodes")]
	public ushort? Episodes { get; init; }

	[JsonPropertyName("chapters")]
	public ushort? Chapters { get; init; }

	[JsonPropertyName("volumes")]
	public ushort? Volumes { get; init; }

	[JsonPropertyName("image")]
	public Image? Image { get; init; }

	[JsonPropertyName("description")]
	public string? Description { get; init; }

	/// <remarks>
	///	Apply <see cref="StringPoolingJsonConverter"/> when https://github.com/dotnet/runtime/issues/54189 gets closed
	/// Currently we cant apply custom converter for collection item
	/// </remarks>>
	[JsonPropertyName("genres")]
	public IReadOnlyList<string> Genres { get; init; } = Array.Empty<string>();

	[JsonPropertyName("tags")]
	public IReadOnlyList<MediaTag> Tags { get; init; } = Array.Empty<MediaTag>();

	[JsonPropertyName("studios")]
	public Connection<Studio> Studios { get; init; } = Connection<Studio>.Empty;

	[JsonPropertyName("characters")]
	public Connection<CharacterEdge> Characters { get; init; } = Connection<CharacterEdge>.Empty;

	[JsonPropertyName("staff")]
	public Connection<StaffEdge> Staff { get; init; } = Connection<StaffEdge>.Empty;
}