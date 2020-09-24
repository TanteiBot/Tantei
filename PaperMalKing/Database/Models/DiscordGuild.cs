using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaperMalKing.Database.Models
{
	[Table("DiscordGuilds")]
	public class DiscordGuild
	{
		[Key]
		[Required]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public long DiscordGuildId { get; set; }

		public long? PostingChannelId { get; set; }

		public List<DiscordGuildUser> Users { get; set; }
	}
}