using System.Text.Json.Serialization;
using PaperMalKing.AniList.Wrapper.Models.Enums;
using PaperMalKing.AniList.Wrapper.Models.Interfaces;

namespace PaperMalKing.AniList.Wrapper.Models
{
    public sealed class Media : IImageble, ISiteUrlable, IIdentifiable
    {
        [JsonPropertyName("id")]
        public ulong Id { get; init; }

        [JsonPropertyName("title")]
        public MediaTitle Title { get; init; } = null!;

        [JsonPropertyName("type")]
        public ListType Type { get; init; }

        [JsonPropertyName("siteUrl")]
        public string Url { get; init; } = null!;

        [JsonPropertyName("format")]
        public MediaFormat Format { get; init; }

        [JsonPropertyName("countryOfOrigin")]
        public string CountryOfOrigin { get; init; } = null!;

        [JsonPropertyName("status")]
        public MediaStatus Status { get; init; }

        [JsonPropertyName("episodes")]
        public ushort? Episodes { get; init; }

        [JsonPropertyName("chapters")]
        public ushort? Chapters { get; init; }

        [JsonPropertyName("volumes")]
        public ushort? Volumes { get; init; }

        [JsonPropertyName("image")]
        public Image Image { get; init; } = null!;
    }
}