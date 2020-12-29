using System.Text.Json.Serialization;

namespace PaperMalKing.Shikimori.Wrapper.Models
{
	internal sealed class User
	{
		private string? _imageUrl;

		[JsonPropertyName("id")]
		public int Id { get; init; }


		[JsonPropertyName("nickname")]
		public string Nickname { get; init; } = null!;

		public string Url => $"{Constants.BASE_URL}/{this.Nickname}";

		public string ImageUrl => this._imageUrl ??= Utils.GetImageUrl("users", this.Id, "png", "x80");

		[JsonPropertyName("stats")]
		public UserStats Stats { get; init; } = null!;

		internal sealed class UserStats
		{
			[JsonPropertyName("has_anime?")]
			public bool HasAnime { get; init; }

			[JsonPropertyName("has_manga?")]
			public bool HasManga { get; init; }
		}
	}
}