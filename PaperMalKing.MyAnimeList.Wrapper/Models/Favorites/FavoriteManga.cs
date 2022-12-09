// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
namespace PaperMalKing.MyAnimeList.Wrapper.Models.Favorites;

internal sealed class FavoriteManga : BaseListFavorite
{
	internal FavoriteManga(string type, int startYear, MalUrl url, string name, string? imageUrl) : base(type,
		startYear, url, name, imageUrl)
	{ }

	internal FavoriteManga(string type, int startYear, BaseFavorite baseFav) : base(type, startYear, baseFav)
	{ }

	internal FavoriteManga(BaseListFavorite other) : base(other)
	{ }
}