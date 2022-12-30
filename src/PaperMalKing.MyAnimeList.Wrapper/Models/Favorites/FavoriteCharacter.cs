// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
namespace PaperMalKing.MyAnimeList.Wrapper.Models.Favorites;

internal sealed class FavoriteCharacter : BaseFavorite
{
	internal string FromName { get; init; }

	internal FavoriteCharacter(string fromName, MalUrl url, string name, string? imageUrl) : base(
		url, name, imageUrl)
	{
		this.FromName = fromName;
	}

	internal FavoriteCharacter(string fromName, BaseFavorite baseFav) : this(fromName,
		baseFav.Url, baseFav.Name, baseFav.ImageUrl)
	{ }
}