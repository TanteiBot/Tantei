using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Models
{
    internal sealed class MediaListTypeOptions
    {
        [JsonPropertyName("advancedScoringEnabled")]
        public bool IsAdvancedScoringEnabled { get; init; }
    }
}