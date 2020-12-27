namespace PaperMalKing.MyAnimeList.Wrapper.Models.Favorites
{
	internal class BaseFavorite
	{
		private readonly string? _imageUrl;
		internal MalUrl Url { get; init; }

		internal string Name { get; init; }

		internal string? ImageUrl
		{
			get => this._imageUrl;
			init => this._imageUrl = value?.ToLargeImage();
		}

		internal BaseFavorite(MalUrl url, string name, string? imageUrl)
		{
			this.Url = url;
			this.Name = name;
			this.ImageUrl = imageUrl;
		}

		internal BaseFavorite(BaseFavorite other) : this(other.Url, other.Name, other.ImageUrl)
		{ }
	}
}