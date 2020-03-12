using Newtonsoft.Json;
using PaperMalKing.MyAnimeList.Jikan.Data.Interfaces;

namespace PaperMalKing.MyAnimeList.Jikan.Data.Models
{
	public sealed class MangaListEntry : IListEntry
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

		/// <inheritdoc />
		[JsonProperty(PropertyName = "read_chapters")]
		public int? CompletedSubEntries { get; set; }

		/// <inheritdoc />
		[JsonProperty(PropertyName = "total_chapters")]
		public int? TotalSubEntries { get; set; }

		/// <inheritdoc />
		[JsonProperty(PropertyName = "score")]
		public int Score { get; set; }

		/// <inheritdoc />
		[JsonProperty(PropertyName = "reading_status")]
		public StatusType UsersStatus { get; set; }
	}
}