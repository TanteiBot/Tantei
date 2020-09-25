namespace PaperMalKing.Database.Models.MyAnimeList
{
	public interface IMALUserFavoriteListEntry : IMALUserFavorite
	{
		int StartYear { get; }
		public string Type { get; }
	}
}