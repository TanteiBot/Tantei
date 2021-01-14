using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Models.Responses
{
    internal sealed class FavouritesResponse
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