// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System.Collections.Generic;
using PaperMalKing.AniList.Wrapper.Models;

namespace PaperMalKing.AniList.UpdateProvider.CombinedResponses;

internal sealed class CombinedInitialInfoResponse
{
	public ulong? UserId = null;

	public readonly List<IdentifiableFavourite> Favourites = new();

	public void Add(User user)
	{
		this.UserId ??= user.Id;

		this.Favourites.AddRange(user.Favourites.AllFavourites);
	}
}