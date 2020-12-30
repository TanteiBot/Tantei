using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PaperMalKing.Shikimori.Wrapper.Models
{
	internal sealed class Favourites
	{
		private readonly List<FavouriteEntry> _allFavourites = new();

		internal IReadOnlyList<FavouriteEntry> AllFavourites => this._allFavourites;

		[JsonPropertyName("animes")]
		public IReadOnlyList<FavouriteEntry> Animes
		{
			init
			{
				foreach (var entry in value)
					entry.Type = "animes";
				this._allFavourites.AddRange(value);
			}
		}

		[JsonPropertyName("mangas")]
		public IReadOnlyList<FavouriteEntry> Mangas
		{
			init
			{
				foreach (var entry in value)
					entry.Type = "mangas";
				this._allFavourites.AddRange(value);
			}
		}

		[JsonPropertyName("characters")]
		public IReadOnlyList<FavouriteEntry> Characters
		{
			init
			{
				foreach (var entry in value)
					entry.Type = "characters";
				this._allFavourites.AddRange(value);
			}
		}

		[JsonPropertyName("people")]
		public IReadOnlyList<FavouriteEntry> People
		{
			init
			{
				foreach (var entry in value)
					entry.Type = "people";
				this._allFavourites.AddRange(value);
			}
		}

		[JsonPropertyName("mangakas")]
		public IReadOnlyList<FavouriteEntry> Mangakas
		{
			init => this.People = value;
		}

		[JsonPropertyName("seyu")]
		public IReadOnlyList<FavouriteEntry> Seyu
		{
			init => this.People = value;
		}

		[JsonPropertyName("producers")]
		public IReadOnlyList<FavouriteEntry> Producers
		{
			init => this.People = value;
		}

		internal sealed class FavouriteEntry
		{
			[JsonIgnore]
			public string? Type { get; set; }

			[JsonPropertyName("id")]
			public ulong Id { get; init; }

			[JsonPropertyName("name")]
			public string Name { get; init; } = null!;

			public string? ImageUrl => Utils.GetImageUrl(this.Type!, this.Id);

			public string? Url => Utils.GetUrl(this.Type!, this.Id);
		}
	}
}