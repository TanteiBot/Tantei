#region LICENSE
// PaperMalKing.
// Copyright (C) 2021 N0D4N
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

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