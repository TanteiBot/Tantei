using System.Text.Json.Serialization;
using Humanizer;

namespace PaperMalKing.Shikimori.Wrapper.Models.List
{
	internal sealed class MangaListSubEntry : BaseListSubEntry
	{
		protected override string Type => "mangas";

		/// <inheritdoc />
		public override string TotalAmount => $"{"ch".ToQuantity(this.Chapters)}, {"v".ToQuantity(this.Volumes)}";

		[JsonPropertyName("volumes")]
		public int Volumes { get; init; }

		[JsonPropertyName("chapters")]
		public int Chapters { get; init; }
	}
}