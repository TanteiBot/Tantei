namespace PaperMalKing.MyAnimeList.Wrapper.Models.Progress
{
	internal enum GenericProgress : byte
	{
		Unknown = 0,

		CurrentlyInProgress = 1,

		Completed = 2,

		OnHold = 3,

		Dropped = 4,

		InPlans = 6,

		Reprogressing = 7,

		All = byte.MaxValue
	}
}