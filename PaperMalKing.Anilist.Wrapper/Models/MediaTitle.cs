using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using PaperMalKing.AniList.Wrapper.Models.Enums;

namespace PaperMalKing.AniList.Wrapper.Models
{
    internal sealed class MediaTitle
    {
        private readonly Dictionary<TitleLanguage, string?> _titles = new(6);

        [JsonPropertyName("stylisedRomaji")]
        internal string? StylisedRomaji
        {
            [Obsolete("", true)] get => throw new NotSupportedException();
            init => this._titles.Add(TitleLanguage.ROMAJI_STYLISED, value);
        }

        [JsonPropertyName("romaji")]
        internal string? Romaji
        {
            [Obsolete("", true)] get => throw new NotSupportedException();
            init => this._titles.Add(TitleLanguage.ROMAJI, value);
        }

        [JsonPropertyName("stylisedEnglish")]
        internal string? StylisedEnglish
        {
            [Obsolete("", true)] get => throw new NotSupportedException();
            init => this._titles.Add(TitleLanguage.ENGLISH_STYLISED, value);
        }

        [JsonPropertyName("english")]
        internal string? English
        {
            [Obsolete("", true)] get => throw new NotSupportedException();
            init => this._titles.Add(TitleLanguage.ENGLISH, value);
        }

        [JsonPropertyName("stylisedNative")]
        internal string? StylisedNative
        {
            [Obsolete("", true)] get => throw new NotSupportedException();
            init => this._titles.Add(TitleLanguage.NATIVE_STYLISED, value);
        }

        [JsonPropertyName("native")]
        internal string? Native
        {
            [Obsolete("", true)] get => throw new NotSupportedException();
            init => this._titles.Add(TitleLanguage.NATIVE, value);
        }

        public string GetTitle(TitleLanguage titleLanguage)
        {
            var title = "";
            for (; titleLanguage != TitleLanguage.NATIVE - 1; titleLanguage--)
            {
                title = this._titles[titleLanguage];
                if (!string.IsNullOrEmpty(title)) break;
            }

            return title!;
        }
    }
}