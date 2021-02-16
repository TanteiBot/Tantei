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

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using PaperMalKing.AniList.Wrapper.Models.Enums;
using PaperMalKing.AniList.Wrapper.Models.Interfaces;

namespace PaperMalKing.AniList.Wrapper.Models
{
    public sealed class Favourites
    {
        public bool HasNextPage { get; init; } = false;

        private readonly List<IdentifiableFavourite> _allFavourites = new();

        public IReadOnlyList<IdentifiableFavourite> AllFavourites => this._allFavourites;

        [JsonPropertyName("anime")]
        public Connection<IdentifiableFavourite> Anime
        {
            [Obsolete("",true)]get => null!;
            init
            {
                if (value.PageInfo.HasNextPage) this.HasNextPage = value.PageInfo.HasNextPage;
                Array.ForEach(value.Nodes, fav => fav.Type = FavouriteType.Anime);
                this._allFavourites.AddRange(value.Nodes);
            }
        }

        [JsonPropertyName("manga")]
        public Connection<IdentifiableFavourite> Manga
        {
            [Obsolete("",true)]get => null!;
            init
            {
                if (value.PageInfo.HasNextPage) this.HasNextPage = value.PageInfo.HasNextPage;

                Array.ForEach(value.Nodes, fav => fav.Type = FavouriteType.Manga);
                this._allFavourites.AddRange(value.Nodes);
            }
        }

        [JsonPropertyName("characters")]
        public Connection<IdentifiableFavourite> Characters
        {
            [Obsolete("",true)]get => null!;
            init
            {
                if (value.PageInfo.HasNextPage) this.HasNextPage = value.PageInfo.HasNextPage;

                Array.ForEach(value.Nodes, fav => fav.Type = FavouriteType.Characters);
                this._allFavourites.AddRange(value.Nodes);
            }
        }

        [JsonPropertyName("staff")]
        public Connection<IdentifiableFavourite> Staff
        {
            [Obsolete("",true)]get => null!;
            init
            {
                if (value.PageInfo.HasNextPage) this.HasNextPage = value.PageInfo.HasNextPage;

                Array.ForEach(value.Nodes, fav => fav.Type = FavouriteType.Staff);
                this._allFavourites.AddRange(value.Nodes);
            }
        }

        [JsonPropertyName("studios")]
        public Connection<IdentifiableFavourite> Studios
        {
            [Obsolete("",true)]get => null!;
            init
            {
                if (value.PageInfo.HasNextPage) this.HasNextPage = value.PageInfo.HasNextPage;

                Array.ForEach(value.Nodes, fav => fav.Type = FavouriteType.Studios);
                this._allFavourites.AddRange(value.Nodes);
            }
        }

        public static readonly Favourites Empty = new() {HasNextPage = false};

        public sealed class IdentifiableFavourite : IIdentifiable, IEquatable<IdentifiableFavourite>
        {
            [JsonPropertyName("id")]
            public ulong Id { get; init; }

            [JsonIgnore]
            public FavouriteType Type { get; set; }

            public bool Equals(IdentifiableFavourite? other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return this.Id == other.Id && this.Type == other.Type;
            }

            public override bool Equals(object? obj)
            {
                return ReferenceEquals(this, obj) || obj is IdentifiableFavourite other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(this.Id, (int) this.Type);
            }
        }
    }
}