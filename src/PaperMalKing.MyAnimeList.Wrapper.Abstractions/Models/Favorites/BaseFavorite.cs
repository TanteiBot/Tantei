// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Favorites;

public class BaseFavorite(MalUrl url, string name, string? imageUrl)
{
	public MalUrl Url { get; init; } = url;

	public string Name { get; init; } = name;

	public string? ImageUrl { get; init; } = imageUrl;

	protected BaseFavorite(BaseFavorite other)
		: this(other.Url, other.Name, other.ImageUrl)
	{
	}
}