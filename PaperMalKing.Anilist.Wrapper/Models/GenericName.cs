using System.Text.Json.Serialization;
using PaperMalKing.AniList.Wrapper.Models.Enums;

namespace PaperMalKing.AniList.Wrapper.Models
{
    internal sealed class GenericName
    {
        [JsonPropertyName("full")]
        internal string? Full { get; init; }

        [JsonPropertyName("native")]
        internal string Native { get; init; } = null!;

        public string GetName(TitleLanguage language)
        {
            return language switch
            {
                TitleLanguage.ROMAJI_STYLISED when this.Full != null => this.Full,
                TitleLanguage.ENGLISH when this.Full != null => this.Full,
                TitleLanguage.ENGLISH_STYLISED when this.Full != null => this.Full,
                TitleLanguage.ROMAJI when this.Full != null => this.Full,
                _ => this.Native
            };
        }
    }
}