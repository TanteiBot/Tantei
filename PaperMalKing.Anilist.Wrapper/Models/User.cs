using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Models
{
    internal sealed class User
    {
        [JsonPropertyName("id")]
        public ulong Id { get; init; }

        [JsonPropertyName("name")]
        public string Name { get; init; } = null!;

        [JsonPropertyName("siteUrl")]
        public string Url { get; init; } = null!;

        [JsonPropertyName("image")]
        public Image Image { get; init; } = null!;

        [JsonPropertyName("options")]
        public UserOptions Options { get; init; } = null!;

        [JsonPropertyName("mediaListOptions")]
        public MediaListOptions MediaListOptions { get; init; } = null!;

        [JsonPropertyName("favourites")]
        public Favourites Favourites { get; init; } = null!;
    }
}