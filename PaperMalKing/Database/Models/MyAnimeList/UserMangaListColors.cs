using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaperMalKing.Database.Models.MyAnimeList
{
	[Table("MyAnimeListUserMangaListColors")]
	public class UserMangaListColors : IUserListColors
	{
		public User User { get; set; } = null!;

		[Key]
		public long UserId { get; set; }

		public int ReadingColor { get; set; }

		public int CompletedColor { get; set; }

		public int OnHoldColor { get; set; }

		public int DroppedColor { get; set; }

		public int PlanToReadColor { get; set; }

		int IUserListColors.InPlansColor => this.PlanToReadColor;

		int IUserListColors.InProgressColor => this.ReadingColor;
	}
}