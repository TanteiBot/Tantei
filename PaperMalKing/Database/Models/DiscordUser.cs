using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaperMalKing.Database.Models
{
	[Table("DiscordUsers")]
	public class DiscordUser
	{
		[Key]
		[Required]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public long DiscordUserId { get; set; }
		
		public virtual List<DiscordGuildUser> Guilds { get; set; }
	}
}