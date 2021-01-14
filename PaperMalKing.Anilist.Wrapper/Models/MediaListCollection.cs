using System;
using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Models
{
    internal sealed class MediaListCollection
    {
        [JsonPropertyName("lists")]
        public MediaListGroup[] Lists { get; init; } = Array.Empty<MediaListGroup>();

        internal sealed class MediaListGroup
        {
            public MediaListEntry[] Entries { get; init; } = Array.Empty<MediaListEntry>();
        }
    }
}