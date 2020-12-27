namespace PaperMalKing.MyAnimeList.Wrapper.Models.Favorites
{
	internal sealed class FavoriteAnime : BaseListFavorite
	{
		internal FavoriteAnime(string type, int startYear, MalUrl url, string name, string? imageUrl) : base(type,
			startYear, url, name, imageUrl)
		{ }

		internal FavoriteAnime(string type, int startYear, BaseFavorite baseFav) : base(type, startYear, baseFav)
		{ }

		internal FavoriteAnime(BaseListFavorite other) : base(other)
		{ }
	}
}