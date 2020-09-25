using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaperMalKing.Database.Models.MyAnimeList
{
	[Table("MyAnimeListUserAnimeListColors")]
	public class MALUserAnimeListColors : IUserListColors
	{
		[ForeignKey("UserId")]
		public MALUser MALUser { get; set; } = null!;

		[Key]
		public long UserId { get; set; }

		public int WatchingColor { get; set; }

		public int CompletedColor { get; set; }

		public int OnHoldColor { get; set; }

		public int DroppedColor { get; set; }
		
		public int PlanToWatchColor { get; set; }

		int IUserListColors.InPlansColor => this.PlanToWatchColor;
		
		int IUserListColors.InProgressColor => this.WatchingColor;
	}
}