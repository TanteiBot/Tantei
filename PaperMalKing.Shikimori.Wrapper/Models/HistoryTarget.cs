// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System;
using System.Text.Json.Serialization;
using PaperMalKing.Common.Enums;

namespace PaperMalKing.Shikimori.Wrapper.Models;

internal class HistoryTarget
{
	private readonly string _url = null!;

	public ListEntryType Type { get; init; }

	[JsonPropertyName("status")]
	public string Status { get; init; } = null!;

	[JsonPropertyName("id")]
	public ulong Id { get; init; }

	[JsonPropertyName("url")]
	public string Url
	{
		get => this._url;
		init
		{
			this._url = $"{Constants.BASE_URL}{value}";
			this.Type = value.Contains("/animes", StringComparison.OrdinalIgnoreCase) ? ListEntryType.Anime : ListEntryType.Manga;
			var entryType = this.Type == ListEntryType.Anime ? "animes" : "mangas";
			this.ImageUrl = Utils.GetImageUrl(entryType, this.Id);
		}
	}

	[JsonPropertyName("episodes")]
	public int? Episodes { get; init; }

	[JsonPropertyName("episodes_aired")]
	public int? EpisodesAired { get; init; }

	[JsonPropertyName("volumes")]
	public int? Volumes { get; init; }

	[JsonPropertyName("chapters")]
	public int? Chapters { get; init; }

	[JsonPropertyName("kind")]
	public string? Kind { get; init; }

	[JsonPropertyName("name")]
	public string Name { get; init; } = null!;

	[JsonPropertyName("russian")]
	public string? RussianName { get; init; } = null!;

	[JsonIgnore]
	public string ImageUrl { get; init; } = null!;
}