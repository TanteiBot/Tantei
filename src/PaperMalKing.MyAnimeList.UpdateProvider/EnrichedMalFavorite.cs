// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using PaperMalKing.Database.Models.MyAnimeList;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Search;

namespace PaperMalKing.MyAnimeList.UpdateProvider;

internal sealed class EnrichedMalFavorite
{
	public required BaseMalFavorite Favorite { get; init; }

	public required bool Added { get; init; }

	public AnimeSearchResult? Anime { get; set; }

	public MangaSearchResult? Manga { get; set; }

	public MediaInfo MediaInfo { get; set; } = MediaInfo.Empty;

	public IReadOnlyList<SeyuInfo> Seiyu { get; set; } = [];

	public EntityInfo Entity { get; set; } = EntityInfo.Empty;
}
