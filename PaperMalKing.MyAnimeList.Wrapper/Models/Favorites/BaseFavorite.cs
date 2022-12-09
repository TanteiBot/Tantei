// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
namespace PaperMalKing.MyAnimeList.Wrapper.Models.Favorites;

internal class BaseFavorite
{
	internal MalUrl Url { get; init; }

	internal string Name { get; init; }

	internal string? ImageUrl { get; init; }

	internal BaseFavorite(MalUrl url, string name, string? imageUrl)
	{
		this.Url = url;
		this.Name = name;
		this.ImageUrl = imageUrl;
	}

	internal BaseFavorite(BaseFavorite other) : this(other.Url, other.Name, other.ImageUrl)
	{ }
}