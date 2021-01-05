using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaperMalKing.Database.Models.Shikimori
{
	public sealed class ShikiUser
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public ulong Id { get; init; }

		public ulong LastHistoryEntryId { get; set; }

		public ulong DiscordUserId { get; init; }

		public DiscordUser DiscordUser { get; init; } = null!;

		public List<ShikiFavourite> Favourites { get; init; } = null!;
	}
}