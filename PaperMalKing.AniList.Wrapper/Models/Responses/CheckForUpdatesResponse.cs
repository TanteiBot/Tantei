#region LICENSE
// PaperMalKing.
// Copyright (C) 2021 N0D4N
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Models.Responses;

public sealed class CheckForUpdatesResponse
{
	public bool HasNextPage => this.User.Favourites.HasNextPage ||
							   this.ListActivities.PageInfo.HasNextPage ||
							   this.Reviews.PageInfo.HasNextPage;

	[JsonPropertyName("User")]
	public User User { get; init; } = null!;

	[JsonPropertyName("AnimeList")]
	public MediaListCollection AnimeList { get; init; } = MediaListCollection.Empty;

	[JsonPropertyName("MangaList")]
	public MediaListCollection MangaList { get; init; } = MediaListCollection.Empty;

	[JsonPropertyName("ActivitiesPage")]
	public Page<ListActivity> ListActivities { get; init; } = Page<ListActivity>.Empty;

	[JsonPropertyName("ReviewsPage")]
	public Page<Review> Reviews { get; init; } = Page<Review>.Empty;
}