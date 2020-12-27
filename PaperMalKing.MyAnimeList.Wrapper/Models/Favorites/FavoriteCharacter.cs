namespace PaperMalKing.MyAnimeList.Wrapper.Models.Favorites
{
	internal sealed class FavoriteCharacter : BaseFavorite
	{
		internal MalUrl FromUrl { get; init; }

		internal string FromName { get; init; }

		internal FavoriteCharacter(MalUrl fromUrl, string fromName, MalUrl url, string name, string? imageUrl) : base(
			url, name, imageUrl)
		{
			this.FromUrl = fromUrl;
			this.FromName = fromName;
		}

		internal FavoriteCharacter(MalUrl fromUrl, string fromName, BaseFavorite baseFav) : this(fromUrl, fromName,
			baseFav.Url, baseFav.Name, baseFav.ImageUrl)
		{ }
	}
}