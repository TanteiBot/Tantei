// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Favorites;

public sealed class FavoriteCompany : BaseFavorite
{
	public FavoriteCompany(MalUrl url, string name, string? imageUrl) : base(url, name, imageUrl)
	{ }

	public FavoriteCompany(BaseFavorite other) : base(other)
	{ }
}