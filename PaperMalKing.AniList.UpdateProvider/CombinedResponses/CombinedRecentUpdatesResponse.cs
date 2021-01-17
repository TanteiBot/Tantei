using System.Collections.Generic;
using PaperMalKing.AniList.Wrapper.Models;
using PaperMalKing.AniList.Wrapper.Models.Responses;

namespace PaperMalKing.AniList.UpdateProvider.CombinedResponses
{
    internal sealed class CombinedRecentUpdatesResponse
    {
        public readonly List<Review> Reviews = new();

        public readonly List<ListActivity> Activities = new();

        public User User { get; private set; } = null!;

        public readonly List<MediaListEntry> AnimeList = new(50);

        public readonly List<MediaListEntry> MangaList = new(50);

        public readonly List<Favourites.IdentifiableFavourite> Favourites = new();

        public void Add(CheckForUpdatesResponse response)
        {
            this.User ??= response.User;
            this.Favourites.AddRange(response.User.Favourites.AllFavourites);
            
            this.Reviews.AddRange(response.Reviews.Values);
            
            this.Activities.AddRange(response.ListActivities.Values);
            foreach (var mediaListGroup in response.AnimeList.Lists)
            {
                this.AnimeList.AddRange(mediaListGroup.Entries);    
            }
            
            foreach (var mediaListGroup in response.MangaList.Lists)
            {
                this.MangaList.AddRange(mediaListGroup.Entries);
            }
        }
    }
}