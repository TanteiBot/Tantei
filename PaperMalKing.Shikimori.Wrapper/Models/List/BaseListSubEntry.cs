using System.Text.Json.Serialization;

namespace PaperMalKing.Shikimori.Wrapper.Models.List
{
	internal abstract class BaseListSubEntry
	{
		protected abstract string Type { get; }

		public abstract string TotalAmount { get; }

		[JsonPropertyName("id")]
		public ulong Id { get; init; }

		[JsonPropertyName("name")]
		public string Name { get; init; } = null!;

		[JsonPropertyName("kind")]
		public string Kind { get; init; } = null!;

		[JsonPropertyName("status")]
		public string Status { get; init; } = null!;

		public string Url => Utils.GetUrl(this.Type, this.Id);

		public string ImageUrl => Utils.GetImageUrl(this.Type, this.Id);
	}
}