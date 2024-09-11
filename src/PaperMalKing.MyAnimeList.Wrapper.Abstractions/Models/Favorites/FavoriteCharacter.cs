// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Favorites;

public sealed class FavoriteCharacter(string fromName, MalUrl url, string name, string? imageUrl) : BaseFavorite(url, name, imageUrl)
{
	public string FromName { get; init; } = fromName;

	public FavoriteCharacter(string fromName, BaseFavorite baseFav)
		: this(fromName, baseFav.Url, baseFav.Name, baseFav.ImageUrl)
	{
	}
}