// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System.Text.Json.Serialization;
using PaperMalKing.AniList.Wrapper.Models.Enums;

namespace PaperMalKing.AniList.Wrapper.Models;

public sealed class MediaListOptions
{
	[JsonPropertyName("scoreFormat")]
	public ScoreFormat ScoreFormat { get; init; }

	[JsonPropertyName("animeList")]
	public required MediaListTypeOptions AnimeListOptions { get; init; }

	[JsonPropertyName("mangaList")]
	public required MediaListTypeOptions MangaListOptions { get; init; }
}