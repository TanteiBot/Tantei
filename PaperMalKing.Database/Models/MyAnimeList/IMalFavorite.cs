namespace PaperMalKing.Database.Models.MyAnimeList
{
	public interface IMalFavorite
	{
		public int UserId { get; init; }

		public int Id { get; init; }
		
		public string? ImageUrl { get; init; }

		public string Name { get; init; }

		public string NameUrl { get; init; }

		public MalUser User { get; init; }
	}
}