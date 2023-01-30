// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System;
using System.Text.Json.Serialization;
using PaperMalKing.Common.Enums;
using PaperMalKing.Common.Json;

namespace PaperMalKing.Shikimori.Wrapper.Abstractions.Models;

public sealed class HistoryTarget : IMultiLanguageName
{
	private string _url = null!;

	public ListEntryType Type { get; internal set; }

	[JsonPropertyName("status")]
	[JsonConverter(typeof(StringPoolingJsonConverter))]
	[JsonRequired]
	public string Status { get; internal set; } = null!;

	[JsonPropertyName("id")]
	public ulong Id { get; internal set; }

	[JsonPropertyName("url")]
	public string Url
	{
		get => this._url;
		internal set
		{
			this._url = $"{Constants.BASE_URL}{value}";
			this.Type = value.Contains("/animes", StringComparison.OrdinalIgnoreCase) ? ListEntryType.Anime : ListEntryType.Manga;
			var entryType = this.Type == ListEntryType.Anime ? "animes" : "mangas";
			this.ImageUrl = Utils.GetImageUrl(entryType, this.Id);
		}
	}

	[JsonPropertyName("episodes")]
	public uint? Episodes { get; internal set; }

	[JsonPropertyName("episodes_aired")]
	public uint? EpisodesAired { get; internal set; }

	[JsonPropertyName("volumes")]
	public uint? Volumes { get; internal set; }

	[JsonPropertyName("chapters")]
	public uint? Chapters { get; internal set; }

	[JsonPropertyName("kind")]
	[JsonConverter(typeof(StringPoolingJsonConverter))]
	public string? Kind { get; internal set; }

	[JsonPropertyName("name")]
	[JsonRequired]
	public string Name { get; internal set; } = null!;

	[JsonPropertyName("russian")]
	public string? RussianName { get; internal set; }

	[JsonIgnore]
	public string ImageUrl { get; internal set; } = null!;
}