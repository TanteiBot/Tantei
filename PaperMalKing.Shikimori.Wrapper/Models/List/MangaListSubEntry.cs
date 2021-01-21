using System.Text.Json.Serialization;

namespace PaperMalKing.Shikimori.Wrapper.Models.List
{
	internal sealed class MangaListSubEntry : BaseListSubEntry
	{
		protected override string Type => "mangas";

		/// <inheritdoc />
		public override string TotalAmount => $"{this.Chapters.ToString()} ep., {this.Volumes.ToString()} v.";

		[JsonPropertyName("volumes")]
		public int Volumes { get; init; }

		[JsonPropertyName("chapters")]
		public int Chapters { get; init; }
	}
}