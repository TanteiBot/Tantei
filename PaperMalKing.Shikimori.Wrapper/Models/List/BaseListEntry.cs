using System;
using System.Text.Json.Serialization;

namespace PaperMalKing.Shikimori.Wrapper.Models.List
{
	internal abstract class BaseListEntry<T> where T : BaseListSubEntry
	{
		[JsonPropertyName("id")]
		public int Id { get; init; }

		[JsonPropertyName("score")]
		public byte Score { get; init; }

		[JsonPropertyName("status")]
		public Status Status { get; init; }

		[JsonPropertyName("text")]
		public string? Text { get; init; }

		[JsonPropertyName("rewatches")]
		public int Rewatches { get; init; }

		[JsonPropertyName("updated_at")]
		public DateTimeOffset UpdatedAt { get; init; }

		internal abstract T SubEntry { get; init; }
	}
}