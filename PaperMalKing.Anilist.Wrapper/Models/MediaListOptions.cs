using System.Text.Json.Serialization;
using PaperMalKing.AniList.Wrapper.Models.Enums;

namespace PaperMalKing.AniList.Wrapper.Models
{
    internal sealed class MediaListOptions
    {
        [JsonPropertyName("scoreFormat")]
        public ScoreFormat ScoreFormat { get; init; }

        [JsonPropertyName("animeList")]
        public MediaListTypeOptions AnimeListOptions { get; init; } = null!;

        [JsonPropertyName("mangaList")]
        public MediaListTypeOptions MangaListOptions { get; init; } = null!;
    }
}