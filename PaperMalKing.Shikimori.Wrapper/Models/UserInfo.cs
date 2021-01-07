using System.Net;
using System.Text.Json.Serialization;

namespace PaperMalKing.Shikimori.Wrapper.Models
{
	internal class UserInfo
	{
		private string? _imageUrl;

		[JsonPropertyName("id")]
		public ulong Id { get; init; }

		[JsonPropertyName("nickname")]
		public string Nickname { get; init; } = null!;

		public string Url => $"{Constants.BASE_URL}/{WebUtility.UrlEncode(this.Nickname)}";

		public string ImageUrl => this._imageUrl ??= Utils.GetImageUrl("users", this.Id, "png", "x80");
	}
}