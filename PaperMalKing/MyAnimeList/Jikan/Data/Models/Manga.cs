using Newtonsoft.Json;
using PaperMalKing.MyAnimeList.Jikan.Data.Interfaces;

namespace PaperMalKing.MyAnimeList.Jikan.Data.Models
{
	public sealed class Manga : BaseJikanRequest, IMalEntity
	{
		/// <inheritdoc />
		[JsonProperty(PropertyName = "mal_id")]
		public long MalId { get; set; }

		/// <inheritdoc />
		[JsonProperty(PropertyName = "title")]
		public string Title { get; set; }

		/// <inheritdoc />
		[JsonProperty(PropertyName = "image_url")]
		public string ImageUrl { get; set; }

		/// <inheritdoc />
		[JsonProperty(PropertyName = "url")]
		public string Url { get; set; }

		/// <inheritdoc />
		[JsonProperty(PropertyName = "type")]
		public string Type { get; set; }
	}
}