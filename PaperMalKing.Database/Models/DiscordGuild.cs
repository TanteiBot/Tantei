using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaperMalKing.Database.Models
{
	public sealed class DiscordGuild
	{
		[Key]
		[Required]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public ulong DiscordGuildId { get; init; }

		public ulong PostingChannelId { get; set; }

		public ICollection<DiscordUser> Users { get; init; } = null!;
	}
}