// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N
namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Favorites;

public sealed class FavoriteCharacter : BaseFavorite
{
	public  string FromName { get; init; }

	public FavoriteCharacter(string fromName, MalUrl url, string name, string? imageUrl) : base(
		url, name, imageUrl)
	{
		this.FromName = fromName;
	}

	public FavoriteCharacter(string fromName, BaseFavorite baseFav) : this(fromName,
		baseFav.Url, baseFav.Name, baseFav.ImageUrl)
	{ }
}