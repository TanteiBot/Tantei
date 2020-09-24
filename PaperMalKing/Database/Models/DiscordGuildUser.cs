using System.ComponentModel.DataAnnotations.Schema;

namespace PaperMalKing.Database.Models
{
	public class DiscordGuildUser
	{
		[ForeignKey("DiscordUserId")]
		public long DiscordUserId { get; set; }

		public virtual DiscordUser DiscordUser { get; set; }

		[ForeignKey("DiscordGuildId")]
		public long DiscordGuildId { get; set; }

		public virtual DiscordGuild DiscordGuild { get; set; }
	}
}