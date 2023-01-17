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
		init => this.SetTypesThenAddToAll(value, "animes", "Anime");
	}

	[JsonPropertyName("mangas")]
	public FavouriteEntry[] Mangas
	{
		[Obsolete("Used only for json serializer", true)]
		get => throw new NotSupportedException();
		init => this.SetTypesThenAddToAll(value, "mangas", "Manga");
	}

	[JsonPropertyName("characters")]
	public FavouriteEntry[] Characters
	{
		[Obsolete("Used only for json serializer", true)]
		get => throw new NotSupportedException();
		init => this.SetTypesThenAddToAll(value, "characters", "Character");
	}

	[JsonPropertyName("people")]
	public FavouriteEntry[] People
	{
		[Obsolete("Used only for json serializer", true)]
		get => throw new NotSupportedException();
		init => this.SetTypesThenAddToAll(value, "people", "Person");
	}

	[JsonPropertyName("mangakas")]
	public FavouriteEntry[] Mangakas
	{
		[Obsolete("Used only for json serializer", true)]
		get => throw new NotSupportedException();
		init => this.SetTypesThenAddToAll(value, "people", "Mangaka");
	}

	[JsonPropertyName("seyu")]
	public FavouriteEntry[] Seyu
	{
		[Obsolete("Used only for json serializer", true)]
		get => throw new NotSupportedException();
		init => this.SetTypesThenAddToAll(value, "people", "Seyu");
	}

	[JsonPropertyName("producers")]
	public FavouriteEntry[] Producers
	{
		[Obsolete("Used only for json serializer", true)]
		get => throw new NotSupportedException();
		init => this.SetTypesThenAddToAll(value, "people", "Producer");
	}

	[JsonPropertyName("ranobe")]
	public FavouriteEntry[] Ranobe
	{
		[Obsolete("Used only for json serializer", true)]
		get => throw new NotSupportedException();
		init => this.SetTypesThenAddToAll(value, "mangas", "Ranobe");
	}

	private void SetTypesThenAddToAll(FavouriteEntry[] entries, string genericType, string? specificType)
	{
		foreach (var entry in entries)
		{
			entry.GenericType = genericType;
			entry.SpecificType = specificType;
		}
		this._allFavourites.AddRange(entries);
	}

	public static readonly Favourites Empty = new();

	void IJsonOnDeserialized.OnDeserialized()
	{
		this._allFavourites.Sort();
	}
}