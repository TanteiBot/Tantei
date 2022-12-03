#region LICENSE
// PaperMalKing.
// Copyright (C) 2021-2022 N0D4N
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

using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Models.Responses
{
    public sealed class FavouritesResponse
    {
        public bool HasNextPage => this.Anime.PageInfo.HasNextPage ||
                                   this.Manga.PageInfo.HasNextPage ||
                                   this.Characters.PageInfo.HasNextPage ||
                                   this.Staff.PageInfo.HasNextPage ||
                                   this.Studios.PageInfo.HasNextPage;

        [JsonPropertyName("Animes")]
        public Page<Media> Anime { get; init; } = Page<Media>.Empty;

        [JsonPropertyName("Mangas")]
        public Page<Media> Manga { get; init; } = Page<Media>.Empty;

        [JsonPropertyName("Characters")]
        public Page<Character> Characters { get; init; } = Page<Character>.Empty;

        [JsonPropertyName("Staff")]
        public Page<Staff> Staff { get; init; } = Page<Staff>.Empty;

        [JsonPropertyName("Studios")]
        public Page<Studio> Studios { get; init; } = Page<Studio>.Empty;

        public static readonly FavouritesResponse Empty = new();
    }
}