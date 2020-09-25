namespace PaperMalKing.Database.Models.MyAnimeList
{
	public interface IUserListColors
	{
		MALUser MALUser { get; }

		long UserId { get; }

		int InProgressColor { get; }

		int CompletedColor { get; }

		int OnHoldColor { get; }

		int DroppedColor { get; }

		int InPlansColor { get; }
	}
}