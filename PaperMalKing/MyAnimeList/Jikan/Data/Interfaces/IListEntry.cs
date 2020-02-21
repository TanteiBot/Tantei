namespace PaperMalKing.MyAnimeList.Jikan.Data.Interfaces
{
	public interface IListEntry : IMalEntity
	{
		int? CompletedSubEntries { get; set; }

		int? TotalSubEntries { get; set; }

		int Score { get; set; }

		StatusType UsersStatus { get; set; }
	}
}