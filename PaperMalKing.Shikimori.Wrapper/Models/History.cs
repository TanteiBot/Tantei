using System;
using System.Text.Json.Serialization;
using PaperMalKing.Common.Enums;

namespace PaperMalKing.Shikimori.Wrapper.Models
{
	internal sealed class History
	{
		[JsonPropertyName("id")]
		public int Id { get; init; }

		[JsonPropertyName("created_at")]
		public DateTimeOffset CreatedAt { get; init; }

		[JsonPropertyName("description")]
		public string Description { get; init; } = null!;

		public HistoryTarget? Target { get; init; }

		internal class HistoryTarget
		{
			private ListEntryType? _type = null;
			
			[JsonPropertyName("id")]
			public int Id { get; init; }

			[JsonPropertyName("url")]
			internal string Url { get; init; } = null!;

			internal ListEntryType Type => this._type ??=
				this.Url.StartsWith("/anime", StringComparison.InvariantCultureIgnoreCase) ? ListEntryType.Anime : ListEntryType.Manga;
		}
	}
}