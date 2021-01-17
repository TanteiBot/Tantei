using System;
using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Models
{
    public sealed class Page<T>
    {
        [JsonPropertyName("pageInfo")]
        public PageInfo PageInfo { get; init; } = null!;

        [JsonPropertyName("values")]
        public T[] Values { get; init; } = null!;

        public static readonly Page<T> Empty = new()
        {
            PageInfo = new() {HasNextPage = false},
            Values = Array.Empty<T>()
        };
    }
}