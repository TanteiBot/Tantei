// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;
using PaperMalKing.Common;

namespace PaperMalKing.AniList.Wrapper.Abstractions.Models;

public sealed class Favourites
{
	public bool HasNextPage { get; init; }

	private readonly List<IdentifiableFavourite> _allFavourites = new(4);

	public IReadOnlyList<IdentifiableFavourite> AllFavourites => this._allFavourites;

	[JsonPropertyName("anime")]
	public Connection<IdentifiableFavourite> Anime
	{
		[Obsolete("This property is used only for JSON deserialization", true)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		get => ThrowNotSupportedException();
		init
		{
			if (value.PageInfo!.HasNextPage)
			{
				this.HasNextPage = value.PageInfo.HasNextPage;
			}

			value.Nodes.ForEach(fav => fav.Type = FavouriteType.Anime);
			this._allFavourites.AddRange(value.Nodes);
		}
	}

	[JsonPropertyName("manga")]
	public Connection<IdentifiableFavourite> Manga
	{
		[Obsolete("This property is used only for JSON deserialization", true)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		get => ThrowNotSupportedException();
		init
		{
			if (value.PageInfo!.HasNextPage)
			{
				this.HasNextPage = value.PageInfo.HasNextPage;
			}

			value.Nodes.ForEach(fav => fav.Type = FavouriteType.Manga);
			this._allFavourites.AddRange(value.Nodes);
		}
	}

	[JsonPropertyName("characters")]
	public Connection<IdentifiableFavourite> Characters
	{
		[Obsolete("This property is used only for JSON deserialization", true)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		get => ThrowNotSupportedException();
		init
		{
			if (value.PageInfo!.HasNextPage)
			{
				this.HasNextPage = value.PageInfo.HasNextPage;
			}

			value.Nodes.ForEach(fav => fav.Type = FavouriteType.Characters);
			this._allFavourites.AddRange(value.Nodes);
		}
	}

	[JsonPropertyName("staff")]
	public Connection<IdentifiableFavourite> Staff
	{
		[Obsolete("This property is used only for JSON deserialization", true)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		get => ThrowNotSupportedException();
		init
		{
			if (value.PageInfo!.HasNextPage)
			{
				this.HasNextPage = value.PageInfo.HasNextPage;
			}

			value.Nodes.ForEach(fav => fav.Type = FavouriteType.Staff);
			this._allFavourites.AddRange(value.Nodes);
		}
	}

	[JsonPropertyName("studios")]
	public Connection<IdentifiableFavourite> Studios
	{
		[Obsolete("This property is used only for JSON deserialization", true)]
		[EditorBrowsable(EditorBrowsableState.Never)]
		get => ThrowNotSupportedException();
		init
		{
			if (value.PageInfo!.HasNextPage)
			{
				this.HasNextPage = value.PageInfo.HasNextPage;
			}

			value.Nodes.ForEach(fav => fav.Type = FavouriteType.Studios);
			this._allFavourites.AddRange(value.Nodes);
		}
	}

	public static readonly Favourites Empty = new() { HasNextPage = false };

	private static Connection<IdentifiableFavourite> ThrowNotSupportedException()
	{
		throw new NotSupportedException("You shouldn't access this property");
	}
}