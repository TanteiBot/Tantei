using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using PaperMalKing.AniList.Wrapper.Models.Enums;

namespace PaperMalKing.AniList.Wrapper.Models
{
    internal sealed class Favourites
    {
        public bool HasNextPage { get; init; } = false;

        private readonly List<IdentifiableFavourite> _allFavourites = new();

        public IReadOnlyList<IdentifiableFavourite> AllFavourites => this._allFavourites;

        [JsonPropertyName("anime")]
        public Connection<IdentifiableFavourite> Anime
        {
            [Obsolete("", true)] get => throw new NotSupportedException();
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
            [Obsolete("", true)] get => throw new NotSupportedException();
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
            [Obsolete("", true)] get => throw new NotSupportedException();
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
            [Obsolete("", true)] get => throw new NotSupportedException();
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
            [Obsolete("", true)] get => throw new NotSupportedException();
            init
            {
                if (value.PageInfo.HasNextPage) this.HasNextPage = value.PageInfo.HasNextPage;

                Array.ForEach(value.Nodes, fav => fav.Type = FavouriteType.Studios);
                this._allFavourites.AddRange(value.Nodes);
            }
        }

        internal sealed class IdentifiableFavourite
        {
            [JsonPropertyName("id")]
            public ulong Id { get; init; }

            [JsonIgnore]
            public FavouriteType Type { get; set; }
        }
    }
}