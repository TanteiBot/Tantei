using System.Text.Json.Serialization;

namespace PaperMalKing.Shikimori.Wrapper.Models.List
{
	internal sealed class AnimeListSubEntry : BaseListSubEntry
	{
		protected override string Type => "animes";

		/// <inheritdoc />
		public override string TotalAmount => $"{this.Episodes.ToString()} ep.";

		[JsonPropertyName("episodes")]
		public int Episodes { get; init; }
	}
}