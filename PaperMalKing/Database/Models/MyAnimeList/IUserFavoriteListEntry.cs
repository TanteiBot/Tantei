namespace PaperMalKing.Database.Models.MyAnimeList
{
	public interface IUserFavoriteListEntry : IUserFavorite
	{
		int StartYear { get; }
		public string Type { get; }
	}
}