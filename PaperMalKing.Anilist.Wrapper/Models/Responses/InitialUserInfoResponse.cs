using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Models.Responses
{
    internal sealed class InitialUserInfoResponse
    {
        [JsonPropertyName("User")]
        internal User User { get; init; } = null!;
    }
}