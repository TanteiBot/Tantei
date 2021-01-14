using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Models
{
    internal sealed class Review
    {
        [JsonPropertyName("createdAt")]
        public long CreatedAtTimeStamp { get; init; }

        [JsonPropertyName("siteUrl")]
        public string Url { get; init; } = null!;

        [JsonPropertyName("summary")]
        public string? Summary { get; init; }

        [JsonPropertyName("media")]
        public Media Media { get; init; } = null!;
    }
}