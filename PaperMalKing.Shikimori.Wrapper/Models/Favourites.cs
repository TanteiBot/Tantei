using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PaperMalKing.Shikimori.Wrapper.Models
{
	internal sealed class Favourites
	{
		private readonly List<FavouriteEntry> _allFavourites = new(20);

		public IReadOnlyList<FavouriteEntry> AllFavourites => this._allFavourites;

		[JsonPropertyName("animes")]
		public FavouriteEntry[] Animes
		{
			[Obsolete("Used only for json serializer", true)]
			get => throw new NotSupportedException();
			init
			{
				foreach (var entry in value)
					entry.GenericType = "animes";
				this._allFavourites.AddRange(value);
			}
		}

		[JsonPropertyName("mangas")]
		public FavouriteEntry[] Mangas
		{
			[Obsolete("Used only for json serializer", true)]
			get => throw new NotSupportedException();
			init
			{
				foreach (var entry in value)
					entry.GenericType = "mangas";
				this._allFavourites.AddRange(value);
			}
		}

		[JsonPropertyName("characters")]
		public FavouriteEntry[] Characters
		{
			[Obsolete("Used only for json serializer", true)]
			get => throw new NotSupportedException();
			init
			{
				foreach (var entry in value)
					entry.GenericType = "characters";
				this._allFavourites.AddRange(value);
			}
		}

		[JsonPropertyName("people")]
		public FavouriteEntry[] People
		{
			[Obsolete("Used only for json serializer", true)]
			get => throw new NotSupportedException();
			init
			{
				foreach (var entry in value)
					entry.GenericType = "people";
				this._allFavourites.AddRange(value);
			}
		}

		[JsonPropertyName("mangakas")]
		public FavouriteEntry[] Mangakas
		{
			[Obsolete("Used only for json serializer", true)]
			get => throw new NotSupportedException();
			init
			{
				foreach (var entry in value)
				{
					entry.SpecificType = "mangaka";
					entry.GenericType = "people";
				}

				this._allFavourites.AddRange(value);
			}
		}

		[JsonPropertyName("seyu")]
		public FavouriteEntry[] Seyu
		{
			[Obsolete("Used only for json serializer", true)]
			get => throw new NotSupportedException();
			init
			{
				foreach (var entry in value)
				{
					entry.SpecificType = "seyu";
					entry.GenericType = "people";
				}

				this._allFavourites.AddRange(value);
			}
		}

		[JsonPropertyName("producers")]
		public FavouriteEntry[] Producers
		{
			[Obsolete("Used only for json serializer", true)]
			get => throw new NotSupportedException();
			init
			{
				foreach (var entry in value)
				{
					entry.SpecificType = "producer";
					entry.GenericType = "people";
				}

				this._allFavourites.AddRange(value);
			}
		}

		public sealed class FavouriteEntry : IEquatable<FavouriteEntry>
		{
			[JsonIgnore]
			public string? GenericType { get; set; }

			[JsonIgnore]
			public string? SpecificType { get; set; } = null;

			[JsonPropertyName("id")]
			public ulong Id { get; init; }

			[JsonPropertyName("name")]
			public string Name { get; init; } = null!;

			public string? ImageUrl => Utils.GetImageUrl(this.GenericType!, this.Id);

			public string? Url => Utils.GetUrl(this.GenericType!, this.Id);

			/// <inheritdoc />
			public bool Equals(FavouriteEntry? other)
			{
				if (ReferenceEquals(null, other))
					return false;
				if (ReferenceEquals(this, other))
					return true;
				return this.GenericType == other.GenericType && this.Id == other.Id;
			}

			/// <inheritdoc />
			public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is FavouriteEntry other && Equals(other);

			/// <inheritdoc />
			public override int GetHashCode()
			{
				unchecked
				{
					return ((this.GenericType != null ? this.GenericType.GetHashCode() : 0) * 397) ^ this.Id.GetHashCode();
				}
			}

			public static bool operator ==(FavouriteEntry? left, FavouriteEntry? right) => Equals(left, right);

			public static bool operator !=(FavouriteEntry? left, FavouriteEntry? right) => !Equals(left, right);
		}
	}
}