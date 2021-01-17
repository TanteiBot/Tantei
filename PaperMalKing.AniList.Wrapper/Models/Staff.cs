using System.Text.Json.Serialization;
using PaperMalKing.AniList.Wrapper.Models.Interfaces;

namespace PaperMalKing.AniList.Wrapper.Models
{
    public sealed class Staff : IImageble, ISiteUrlable, IIdentifiable
    {
        [JsonPropertyName("name")]
        public GenericName Name { get; init; } = null!;

        [JsonPropertyName("siteUrl")]
        public string Url { get; init; } = null!;

        [JsonPropertyName("image")]
        public Image Image { get; init; } = null!;

        [JsonPropertyName("id")]
        public ulong Id { get; init; }
    }
}