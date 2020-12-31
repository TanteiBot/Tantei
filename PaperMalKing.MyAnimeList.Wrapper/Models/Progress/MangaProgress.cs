using System.ComponentModel;

namespace PaperMalKing.MyAnimeList.Wrapper.Models.Progress
{
	internal enum MangaProgress : byte
	{
		Unknown = 0,

		Reading = 1,

		Completed = 2,

		OnHold = 3,

		Dropped = 4,

		PlanToRead = 6,

		[Description("Re-reading")]
		Rereading = 7,

		All = byte.MaxValue
	}
}