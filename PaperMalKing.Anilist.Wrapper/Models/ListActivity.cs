using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Models
{
    public sealed class ListActivity
    {
        [JsonPropertyName("status")]
        public string Status { get; init; } = null!;

        [JsonPropertyName("progress")]
        public string? Progress { get; init; } = null!;

        [JsonPropertyName("createdAt")]
        public long CreatedAtTimestamp { get; init; }

        [JsonPropertyName("media")]
        public Media Media { get; init; } = null!;
    }
}