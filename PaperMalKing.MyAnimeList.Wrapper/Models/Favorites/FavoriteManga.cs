namespace PaperMalKing.MyAnimeList.Wrapper.Models.Favorites
{
	internal sealed class FavoriteManga : BaseListFavorite
	{
		internal FavoriteManga(string type, int startYear, MalUrl url, string name, string? imageUrl) : base(type,
			startYear, url, name, imageUrl)
		{ }

		internal FavoriteManga(string type, int startYear, BaseFavorite baseFav) : base(type, startYear, baseFav)
		{ }

		internal FavoriteManga(BaseListFavorite other) : base(other)
		{ }
	}
}