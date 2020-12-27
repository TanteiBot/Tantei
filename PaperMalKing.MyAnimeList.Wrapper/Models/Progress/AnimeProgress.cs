namespace PaperMalKing.MyAnimeList.Wrapper.Models.Progress
{
	internal enum AnimeProgress : byte
	{
		Unknown = 0,

		Watching = 1,

		Completed = 2,

		OnHold = 3,

		Dropped = 4,

		PlanToWatch = 6,

		Rewatching = 7,

		All = byte.MaxValue
	}
}