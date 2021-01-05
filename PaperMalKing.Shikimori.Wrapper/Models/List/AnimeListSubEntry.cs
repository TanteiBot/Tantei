using System.Text.Json.Serialization;
using Humanizer;

namespace PaperMalKing.Shikimori.Wrapper.Models.List
{
	internal sealed class AnimeListSubEntry : BaseListSubEntry
	{
		protected override string Type => "animes";

		/// <inheritdoc />
		public override string TotalAmount => "ep".ToQuantity(this.Episodes);

		[JsonPropertyName("episodes")]
		public int Episodes { get; init; }
	}
}