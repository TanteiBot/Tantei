#region LICENSE

// PaperMalKing.
// Copyright (C) 2021 N0D4N
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

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
				{
					entry.GenericType = "animes";
				}

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
				{
					entry.GenericType = "mangas";
				}

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
				{
					entry.GenericType = "characters";
				}
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
				{
					entry.GenericType = "people";
				}
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
					entry.GenericType = "people";
					entry.SpecificType = "Mangaka";
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
					entry.GenericType = "people";
					entry.SpecificType = "Seyu";
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
					entry.GenericType = "people";
					entry.SpecificType = "Producer";
				}

				this._allFavourites.AddRange(value);
			}
		}

		[JsonPropertyName("ranobe")]
		public FavouriteEntry[] Ranobe
		{
			[Obsolete("Used only for json serializer", true)]
			get => throw new NotSupportedException();
			init
			{
				foreach (var entry in value)
				{
					entry.GenericType = "mangas";
					entry.SpecificType = "Ranobe";
				}

				this._allFavourites.AddRange(value);
			}
		}

		public static readonly Favourites Empty = new();
	}
}