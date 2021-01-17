using System.Text.Json.Serialization;
using PaperMalKing.AniList.Wrapper.Models.Enums;

namespace PaperMalKing.AniList.Wrapper.Models
{
    public sealed class UserOptions
    {
        [JsonPropertyName("titleLanguage")]
        public TitleLanguage TitleLanguage { get; init; }
    }
}