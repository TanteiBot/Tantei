using System.Collections.Generic;

namespace PaperMalKing.MyAnimeList.Wrapper.Models.Favorites
{
	internal sealed class UserFavorites
	{
		internal IReadOnlyList<FavoriteAnime> FavoriteAnime { get; init; } = null!;

		internal IReadOnlyList<FavoriteManga> FavoriteManga { get; init; } = null!;

		internal IReadOnlyList<FavoriteCharacter> FavoriteCharacters { get; init; } = null!;

		internal IReadOnlyList<FavoritePerson> FavoritePeople { get; init; } = null!;
	}
}