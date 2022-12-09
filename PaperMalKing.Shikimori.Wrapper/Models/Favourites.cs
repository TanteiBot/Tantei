// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PaperMalKing.Shikimori.Wrapper.Models;

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