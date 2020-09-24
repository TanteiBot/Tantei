namespace PaperMalKing.Database.Models.MyAnimeList
{
	public interface IUserFavorite
	{
		User User { get; }

		long UserId { get; }
		string Name { get; }

		string Url { get; }

		string? ImageUrl { get; }

		long Id { get; }
	}
}