#region LICENSE
// PaperMalKing.
// Copyright (C) 2021 N0D4N
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

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