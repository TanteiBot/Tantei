using System.Text.Json;

namespace PaperMalKing.AniList.Wrapper
{
    public class JsonUpperPolicyCase : JsonNamingPolicy
    {
        public override string ConvertName(string name) => name.ToUpperInvariant();
    }
}