using System.ComponentModel;

namespace PaperMalKing.Shikimori.Wrapper.Models.List
{
	public enum SubEntryReleasingStatus : byte
	{
		[Description("Выпущено")]
		Released = 0,

		[Description("Онгоинг")]
		Ongoing = 2,

		[Description("Анонс")]
		Anons = 3
	}
}