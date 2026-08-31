// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Text.Json.Serialization;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.MangaList;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Search;

public sealed class MangaSearchResult : BaseSearchResult<MangaMediaType, MangaPublishingStatus>
{
	[JsonIgnore]
	public override int? Year => this.StartDate?.Year;

	[JsonPropertyName("num_chapters")]
	public required uint Chapters { get; init; }

	[JsonPropertyName("num_volumes")]
	public required uint Volumes { get; init; }
}
