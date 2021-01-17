using System.Collections.Generic;
using PaperMalKing.AniList.Wrapper.Models;
using PaperMalKing.AniList.Wrapper.Models.Responses;

namespace PaperMalKing.AniList.UpdateProvider.CombinedResponses
{
    public sealed class CombinedFavouritesInfoResponse
    {
        public readonly List<Media> Anime = new();
        public readonly List<Media> Manga = new();
        public readonly List<Character> Characters = new();
        public readonly List<Staff> Staff = new();
        public readonly List<Studio> Studios = new();

        public void Add(FavouritesResponse response)
        {
            this.Anime.AddRange(response.Anime.Values);
            this.Manga.AddRange(response.Manga.Values);
            this.Characters.AddRange(response.Characters.Values);
            this.Staff.AddRange(response.Staff.Values);
            this.Studios.AddRange(response.Studios.Values);
        }
    }
}