namespace PaperMalKing.MyAnimeList
{
	public enum StatusType
	{
		All = 0,

		/// <summary>
		/// Reading / Watching
		/// </summary>
		InProgress = 1,

		Completed = 2,

		OnHold = 3,

		Dropped = 4,

		/// <summary>
		/// Plan to read / Plan to watch
		/// </summary>
		PlanToCheck = 6,

		Undefined = 7
	}
}