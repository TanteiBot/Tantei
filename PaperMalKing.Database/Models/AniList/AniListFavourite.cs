using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaperMalKing.Database.Models.AniList
{
    public sealed class AniListFavourite
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong Id { get; init; }
        
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public FavouriteType FavouriteType { get; init; }
        
        public ulong UserId { get; set; }
        
        public AniListUser User { get; set; } = null!;
    }
}