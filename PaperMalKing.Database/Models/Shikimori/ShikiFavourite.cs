using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaperMalKing.Database.Models.Shikimori
{
	public sealed class ShikiFavourite
	{
		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public ulong Id { get; init; }

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public string FavType { get; init; } = null!;
		
		public ShikiUser User { get; init; } = null!;
	}
}