using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Models
{
    public sealed class Connection<T>
    {
        [JsonPropertyName("pageInfo")]
        public PageInfo PageInfo { get; init; } = null!;

        [JsonPropertyName("values")]
        public T[] Nodes { get; init; } = null!;
    }
}