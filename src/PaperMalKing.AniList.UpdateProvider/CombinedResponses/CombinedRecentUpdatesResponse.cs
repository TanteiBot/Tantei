// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.Collections.Generic;
using PaperMalKing.AniList.Wrapper.Abstractions.Models;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Responses;

namespace PaperMalKing.AniList.UpdateProvider.CombinedResponses;

internal sealed class CombinedRecentUpdatesResponse
{
	private const int AniListMediaLimit = 50;

	public List<Review> Reviews { get; } = [];

	public List<ListActivity> Activities { get; } = [];

	private User? _user;

	public User User => this._user!;

	public List<MediaListEntry> AnimeList { get; } = new(AniListMediaLimit);

	public List<MediaListEntry> MangaList { get; } = new(AniListMediaLimit);

	public List<IdentifiableFavourite> Favourites { get; } = [];

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