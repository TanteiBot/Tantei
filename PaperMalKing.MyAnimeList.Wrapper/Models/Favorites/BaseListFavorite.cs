// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
namespace PaperMalKing.MyAnimeList.Wrapper.Models.Favorites;

internal class BaseListFavorite : BaseFavorite
{
	internal string Type { get; init; }

	internal int StartYear { get; init; }

	internal BaseListFavorite(string type, int startYear, MalUrl url, string name, string? imageUrl) : base(url,
		name, imageUrl)
	{
		this.Type = type;
		this.StartYear = startYear;
	}

	internal BaseListFavorite(string type, int startYear, BaseFavorite baseFav) : this(type, startYear, baseFav.Url,
		baseFav.Name, baseFav.ImageUrl)
	{ }

	internal BaseListFavorite(BaseListFavorite other) : this(other.Type, other.StartYear, other.Url, other.Name,
		other.ImageUrl)
	{ }
}