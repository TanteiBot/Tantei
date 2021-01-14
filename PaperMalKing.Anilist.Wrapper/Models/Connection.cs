using System;
using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Models
{
    internal sealed class Connection<T>
    {
        [JsonPropertyName("pageInfo")]
        public PageInfo PageInfo { get; init; } = null!;

        [JsonPropertyName("nodes")]
        public T[] Nodes { get; init; } = Array.Empty<T>();

    }
}