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