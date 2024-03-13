// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N
namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Favorites;

public sealed class FavoriteManga : BaseListFavorite
{
	public FavoriteManga(string type, ushort startYear, MalUrl url, string name, string? imageUrl)
		: base(type, startYear, url, name, imageUrl)
	{
	}

	public FavoriteManga(string type, ushort startYear, BaseFavorite baseFav)
		: base(type, startYear, baseFav)
	{
	}

	public FavoriteManga(BaseListFavorite other)
		: base(other)
	{
	}
}