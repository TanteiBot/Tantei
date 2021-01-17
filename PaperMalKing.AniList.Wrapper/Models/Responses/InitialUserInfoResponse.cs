using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Models.Responses
{
    public sealed class InitialUserInfoResponse
    {
        [JsonPropertyName("User")]
        public User User { get; init; } = null!;
    }
}