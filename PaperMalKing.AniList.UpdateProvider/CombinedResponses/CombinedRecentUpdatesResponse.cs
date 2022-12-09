// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System.Collections.Generic;
using PaperMalKing.AniList.Wrapper.Models;
using PaperMalKing.AniList.Wrapper.Models.Responses;

namespace PaperMalKing.AniList.UpdateProvider.CombinedResponses;

internal sealed class CombinedRecentUpdatesResponse
{
	public readonly List<Review> Reviews = new();

	public readonly List<ListActivity> Activities = new();

	private User? _user;

	public User User => this._user!;

	public readonly List<MediaListEntry> AnimeList = new(50);

	public readonly List<MediaListEntry> MangaList = new(50);

	public readonly List<IdentifiableFavourite> Favourites = new();

	public void Add(CheckForUpdatesResponse response)
	{
		this._user ??= response.User;
		this.Favourites.AddRange(response.User.Favourites.AllFavourites);

		this.Reviews.AddRange(response.Reviews.Values);

		this.Activities.AddRange(response.ListActivities.Values);
		foreach (var mediaListGroup in response.AnimeList.Lists)
		{
			this.AnimeList.AddRange(mediaListGroup.Entries);
		}

		foreach (var mediaListGroup in response.MangaList.Lists)
		{
			this.MangaList.AddRange(mediaListGroup.Entries);
		}
	}
}