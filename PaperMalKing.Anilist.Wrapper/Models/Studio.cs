using System.Text.Json.Serialization;
using PaperMalKing.AniList.Wrapper.Models.Interfaces;

namespace PaperMalKing.AniList.Wrapper.Models
{
    public sealed class Studio : ISiteUrlable, IIdentifiable
    {
        [JsonPropertyName("name")]
        public string Name { get; init; } = null!;

        [JsonPropertyName("siteUrl")]
        public string Url { get; init; } = null!;

        [JsonPropertyName("media")]
        public Connection<Media> Media { get; init; }

        [JsonPropertyName("id")]
        public ulong Id { get; init; }
    }
}