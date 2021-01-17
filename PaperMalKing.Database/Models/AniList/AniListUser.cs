using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaperMalKing.Database.Models.AniList
{
    public sealed class AniListUser
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public ulong Id { get; init; }
        
        public long LastActivityTimestamp { get; set; }
        
        public long LastReviewTimestamp { get; set; }
        
        public ulong DiscordUserId { get; init; }

        public DiscordUser DiscordUser { get; init; } = null!;

        public List<AniListFavourite> Favourites { get; init; } = null!;
    }
}