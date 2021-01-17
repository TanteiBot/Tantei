using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Models
{
    public sealed class MediaListTypeOptions
    {
        [JsonPropertyName("advancedScoringEnabled")]
        public bool IsAdvancedScoringEnabled { get; init; }
    }
}