using Newtonsoft.Json;

namespace PaperMalKing.MyAnimeList.Jikan.Data.Models
{
	/// <summary>
	/// User's profile model class.
	/// </summary>
	public sealed class UserProfile: BaseJikanRequest
	{
		/// <summary>
		/// Username.
		/// </summary>
		[JsonProperty(PropertyName = "username")]
		public string Username { get; set; }

		/// <summary>
		/// User's image URL
		/// </summary>
		[JsonProperty(PropertyName = "image_url")]
		public string ImageUrl { get; set; }

		/// <summary>
		/// User's URL
		/// </summary>
		[JsonProperty(PropertyName = "url")]
		public string Url { get; set; }

		/// <summary>
		/// User's id.
		/// </summary>
		[JsonProperty(PropertyName = "user_id")]
		public long UserId { get; set; }
	}
}