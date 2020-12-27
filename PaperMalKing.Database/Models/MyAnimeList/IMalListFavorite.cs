namespace PaperMalKing.Database.Models.MyAnimeList
{
	public interface IMalListFavorite : IMalFavorite
	{
		public string Type { get; init; }

		public int StartYear { get; init; }
	}
}