// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
namespace PaperMalKing.MyAnimeList.Wrapper.Models.Favorites;

internal sealed class FavoriteAnime : BaseListFavorite
{
	internal FavoriteAnime(string type, uint startYear, MalUrl url, string name, string? imageUrl) : base(type,
		startYear, url, name, imageUrl)
	{ }

	internal FavoriteAnime(string type, uint startYear, BaseFavorite baseFav) : base(type, startYear, baseFav)
	{ }

	internal FavoriteAnime(BaseListFavorite other) : base(other)
	{ }
}