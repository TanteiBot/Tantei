// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Text.Json.Serialization;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Interfaces;

namespace PaperMalKing.AniList.Wrapper.Abstractions.Models;

public sealed class SearchMedia : IIdentifiable, IImageble, ISiteUrlable, IMediaTextInfo, IMediaTitleInfo, IMediaCountsInfo
{
	[JsonPropertyName("id")]
	public uint Id { get; init; }

	[JsonPropertyName("title")]
	public required MediaTitle Title { get; init; }

	[JsonPropertyName("synonyms")]
	public IReadOnlyList<string> Synonyms { get; init; } = [];

	[JsonPropertyName("type")]
	public ListType Type { get; init; }

	[JsonPropertyName("siteUrl")]
	public required string Url { get; init; }

	[JsonPropertyName("image")]
	public Image? Image { get; init; }

	[JsonPropertyName("format")]
	public MediaFormat? Format { get; init; }

	[JsonPropertyName("isAdult")]
	public bool IsAdult { get; init; }

	[JsonPropertyName("popularity")]
	public uint Popularity { get; init; }

	[JsonPropertyName("status")]
	public MediaStatus Status { get; init; }

	[JsonPropertyName("episodes")]
	public ushort? Episodes { get; init; }

	[JsonPropertyName("chapters")]
	public ushort? Chapters { get; init; }

	[JsonPropertyName("volumes")]
	public ushort? Volumes { get; init; }

	[JsonPropertyName("averageScore")]
	public ushort? AverageScore { get; init; }

	[JsonPropertyName("seasonYear")]
	public ushort? SeasonYear { get; init; }

	[JsonPropertyName("season")]
	public MediaSeason? Season { get; init; }

	[JsonPropertyName("description")]
	public string? Description { get; init; }

	[JsonPropertyName("tags")]
	public IReadOnlyList<MediaTag> Tags { get; init; } = [];

	string? IMediaTitleInfo.CountryOfOrigin => null;
}
