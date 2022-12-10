// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System;
using System.Collections.Generic;

namespace PaperMalKing.MyAnimeList.Wrapper.Models.Favorites;

internal sealed class UserFavorites
{
	internal required IReadOnlyList<FavoriteAnime> FavoriteAnime { get; init; }

	internal required IReadOnlyList<FavoriteManga> FavoriteManga { get; init; }

	internal required IReadOnlyList<FavoriteCharacter> FavoriteCharacters { get; init; }

	internal required IReadOnlyList<FavoritePerson> FavoritePeople { get; init; }

	internal required IReadOnlyList<FavoriteCompany> FavoriteCompanies { get; init; }

	internal static readonly UserFavorites Empty = new()
	{
		FavoriteAnime = Array.Empty<FavoriteAnime>(),
		FavoriteManga = Array.Empty<FavoriteManga>(),
		FavoriteCharacters = Array.Empty<FavoriteCharacter>(),
		FavoritePeople = Array.Empty<FavoritePerson>(),
		FavoriteCompanies = Array.Empty<FavoriteCompany>()
	};
}