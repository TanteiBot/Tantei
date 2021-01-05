using System.ComponentModel;

namespace PaperMalKing.Shikimori.Wrapper.Models
{
	public enum AnimeStatus : byte
	{
		[Description("Смотрю")]
		Watching = 0,
		[Description("Просмотрено")]
		Completed = 1,
		[Description("Отложено")]
		OnHold = 2,
		[Description("Брошено")]
		Dropped = 3,
		[Description("Запланировано")]
		Planned = 4,
		[Description("Пересматриваю")]
		Rewatching = 5
	}
}