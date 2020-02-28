namespace PaperMalKing.MyAnimeList.Jikan.Data.Interfaces
{
	/// <summary>
	/// Entity in user's mangalist or animelist
	/// </summary>
	public interface IListEntry : IMalEntity
	{
		/// <summary>
		/// Amount of user's completed subentries i.e. chapters read or episodes watched
		/// </summary>
		int? CompletedSubEntries { get; set; }

		/// <summary>
		/// Amount of entries overall subentries
		/// </summary>
		int? TotalSubEntries { get; set; }

		/// <summary>
		/// User's score for entity
		/// </summary>
		int Score { get; set; }

		/// <summary>
		/// User's status for entity
		/// </summary>
		StatusType UsersStatus { get; set; }
	}
}