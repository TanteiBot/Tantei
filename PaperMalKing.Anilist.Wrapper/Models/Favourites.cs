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