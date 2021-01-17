using System.Text.Json.Serialization;
using PaperMalKing.AniList.Wrapper.Models.Enums;
using PaperMalKing.AniList.Wrapper.Models.Interfaces;

namespace PaperMalKing.AniList.Wrapper.Models
{
    public sealed class Review : ISiteUrlable
    {
        [JsonPropertyName("createdAt")]
        public long CreatedAtTimeStamp { get; init; }

        [JsonPropertyName("siteUrl")]
        public string Url { get; init; } = null!;

        [JsonPropertyName("summary")]
        public string? Summary { get; init; }

        [JsonPropertyName("media")]
        public Media Media { get; init; } = null!;
        
        [JsonPropertyName("format")]
        public MediaFormat Format { get; init; }
    }
}