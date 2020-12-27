using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaperMalKing.Database.Models
{
	public sealed class BotUser
	{
		[Key]
		[Required]
		[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
		public long UserId { get; internal init; }

		public DiscordUser DiscordUser { get; init; } = null!;
	}
}