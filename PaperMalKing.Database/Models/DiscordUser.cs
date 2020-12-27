using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaperMalKing.Database.Models
{
	public sealed class DiscordUser
	{
		private long BotUserId { get; set; }

		[Key]
		[Required]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public ulong DiscordUserId { get; init; }

		[ForeignKey(nameof(BotUserId))]
		public BotUser BotUser { get; set; } = null!;

		public ICollection<DiscordGuild> Guilds { get; init; } = null!;
	}
}