// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PaperMalKing.Shikimori.Wrapper.Models;

internal sealed class Favourites : IJsonOnDeserialized
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
				entry.SpecificType = "Anime";
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
				entry.SpecificType = "Manga";
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
				entry.SpecificType = "Character";
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
				entry.SpecificType = "Person";
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

	void IJsonOnDeserialized.OnDeserialized()
	{
		this._allFavourites.Sort();
	}
}