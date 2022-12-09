// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System;
using System.Collections.Generic;

namespace PaperMalKing.MyAnimeList.Wrapper.Models.Favorites
{
	internal sealed class UserFavorites
	{
		internal IReadOnlyList<FavoriteAnime> FavoriteAnime { get; init; } = null!;

		internal IReadOnlyList<FavoriteManga> FavoriteManga { get; init; } = null!;

		internal IReadOnlyList<FavoriteCharacter> FavoriteCharacters { get; init; } = null!;

		internal IReadOnlyList<FavoritePerson> FavoritePeople { get; init; } = null!;

		internal IReadOnlyList<FavoriteCompany> FavoriteCompanies { get; init; } = null!;

		internal static readonly UserFavorites Empty = new()
		{
			FavoriteAnime = Array.Empty<FavoriteAnime>(),
			FavoriteManga = Array.Empty<FavoriteManga>(),
			FavoriteCharacters = Array.Empty<FavoriteCharacter>(),
			FavoritePeople = Array.Empty<FavoritePerson>(),
			FavoriteCompanies = Array.Empty<FavoriteCompany>()
		};
	}
}