// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Favorites;

public class BaseFavorite
{
	public MalUrl Url { get; init; }

	public string Name { get; init; }

	public string? ImageUrl { get; init; }

	public BaseFavorite(MalUrl url, string name, string? imageUrl)
	{
		this.Url = url;
		this.Name = name;
		this.ImageUrl = imageUrl;
	}

	public BaseFavorite(BaseFavorite other)
		: this(other.Url, other.Name, other.ImageUrl)
	{
	}
}