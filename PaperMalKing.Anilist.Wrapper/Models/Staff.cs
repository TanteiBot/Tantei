using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Models
{
    internal sealed class Staff
    {
        [JsonPropertyName("name")]
        public GenericName Name { get; init; } = null!;

        [JsonPropertyName("siteUrl")]
        public string Url { get; init; } = null!;

        [JsonPropertyName("image")]
        public Image Image { get; init; } = null!;
    }
}