// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Abstractions.Models.Responses;

public sealed class CheckForUpdatesResponse
{
	public bool HasNextPage => this.User.Favourites.HasNextPage ||
							   this.ListActivities.PageInfo!.HasNextPage ||
							   this.Reviews.PageInfo!.HasNextPage;

	[JsonPropertyName("User")]
	public required User User { get; init; }

	[JsonPropertyName("AnimeList")]
	public MediaListCollection AnimeList { get; init; } = MediaListCollection.Empty;

	[JsonPropertyName("MangaList")]
	public MediaListCollection MangaList { get; init; } = MediaListCollection.Empty;

	[JsonPropertyName("ActivitiesPage")]
	public Page<ListActivity> ListActivities { get; init; } = Page<ListActivity>.Empty;

	[JsonPropertyName("ReviewsPage")]
	public Page<Review> Reviews { get; init; } = Page<Review>.Empty;
}