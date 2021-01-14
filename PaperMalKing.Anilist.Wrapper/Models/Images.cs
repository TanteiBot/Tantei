using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Models
{
    internal sealed class Image
    {
        [JsonPropertyName("large")]
        public string ImageUrl { get; init; } = null!;
    }
}