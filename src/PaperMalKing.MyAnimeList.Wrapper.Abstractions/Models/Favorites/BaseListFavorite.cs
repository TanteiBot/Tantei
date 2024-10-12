// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Favorites;

public class BaseListFavorite(string type, ushort startYear, MalUrl url, string name, string? imageUrl) : BaseFavorite(url, name, imageUrl)
{
	public string Type { get; init; } = type;

	public ushort StartYear { get; init; } = startYear;

	public BaseListFavorite(string type, ushort startYear, BaseFavorite baseFav)
		: this(type, startYear, baseFav.Url, baseFav.Name, baseFav.ImageUrl)
	{
	}

	public BaseListFavorite(BaseListFavorite other)
		: this(other.Type, other.StartYear, other.Url, other.Name, other.ImageUrl)
	{
	}
}