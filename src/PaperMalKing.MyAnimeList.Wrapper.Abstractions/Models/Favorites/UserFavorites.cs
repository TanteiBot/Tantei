// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using System.Collections.Generic;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Favorites;

public sealed class UserFavorites
{
	public  required IReadOnlyList<FavoriteAnime> FavoriteAnime { get; init; }

	public  required IReadOnlyList<FavoriteManga> FavoriteManga { get; init; }

	public  required IReadOnlyList<FavoriteCharacter> FavoriteCharacters { get; init; }

	public  required IReadOnlyList<FavoritePerson> FavoritePeople { get; init; }

	public  required IReadOnlyList<FavoriteCompany> FavoriteCompanies { get; init; }

	public  static readonly UserFavorites Empty = new()
	{
		FavoriteAnime = Array.Empty<FavoriteAnime>(),
		FavoriteManga = Array.Empty<FavoriteManga>(),
		FavoriteCharacters = Array.Empty<FavoriteCharacter>(),
		FavoritePeople = Array.Empty<FavoritePerson>(),
		FavoriteCompanies = Array.Empty<FavoriteCompany>()
	};

	public  int Count =>
		this.FavoriteAnime.Count + this.FavoriteManga.Count + this.FavoriteCharacters.Count + this.FavoritePeople.Count + this.FavoriteCompanies.Count;
}