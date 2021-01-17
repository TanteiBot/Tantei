using System.Collections.Generic;
using PaperMalKing.AniList.Wrapper.Models;

namespace PaperMalKing.AniList.UpdateProvider.CombinedResponses
{
    internal sealed class CombinedInitialInfoResponse
    {
        public ulong? UserId = null;

        public readonly List<Favourites.IdentifiableFavourite> Favourites = new();

        public void Add(User user)
        {
            this.UserId ??= user.Id;
            
            this.Favourites.AddRange(user.Favourites.AllFavourites);
        }
    }
}