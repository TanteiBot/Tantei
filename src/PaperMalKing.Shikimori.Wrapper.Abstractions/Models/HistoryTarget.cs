// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using PaperMalKing.Common.Enums;
using PaperMalKing.Common.Json;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Media;

namespace PaperMalKing.Shikimori.Wrapper.Abstractions.Models;

public sealed class HistoryTarget : IMultiLanguageName
{
	public const string ImageFormat = "jpg";

	public ListEntryType Type { get; init; }

	[JsonPropertyName("status")]
	[JsonConverter(typeof(StringPoolingJsonConverter))]
	public required string Status { get; init; }

	[JsonPropertyName("id")]
	public ulong Id { get; init; }

	[JsonPropertyName("url")]
	[field: MaybeNull]
	public string Url
	{
		get;
		init
		{
			field = Constants.BaseUrl + value;
			this.Type = value.Contains("/animes", StringComparison.OrdinalIgnoreCase) ? ListEntryType.Anime : ListEntryType.Manga;
		}
	}

	[JsonPropertyName("episodes")]
	public uint? Episodes { get; init; }

	[JsonPropertyName("episodes_aired")]
	public uint? EpisodesAired { get; init; }

	[JsonPropertyName("volumes")]
	public uint? Volumes { get; init; }

	[JsonPropertyName("chapters")]
	public uint? Chapters { get; init; }

	[JsonPropertyName("kind")]
	[JsonConverter(typeof(StringPoolingJsonConverter))]
	public string? Kind { get; init; }

	[JsonPropertyName("name")]
	public required string Name { get; init; }

	[JsonPropertyName("russian")]
	public string? RussianName { get; init; }

	[JsonPropertyName("image")]
	public HistoryTargetImage? Image { get; init; }

	[JsonIgnore]
	public string? ImageUrl => this.Image?.ImageUrl;
}