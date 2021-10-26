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
using PaperMalKing.Common.Converters;
using PaperMalKing.MyAnimeList.Wrapper.Models.Progress;
using PaperMalKing.MyAnimeList.Wrapper.Models.Status;

namespace PaperMalKing.MyAnimeList.Wrapper.Models.List;

internal sealed class AnimeListEntry : IListEntry
{
	private readonly string _url = null!;
	private readonly AnimeProgress _userAnimeProgress;
	private readonly string _imageUrl = null!;

	[JsonPropertyName("status")]
	public AnimeProgress UserAnimeProgress
	{
		get => this.IsRewatching ? AnimeProgress.Rewatching : this._userAnimeProgress;
		init => this._userAnimeProgress = value;
	}

	[JsonPropertyName("num_watched_episodes")]
	public int WatchedEpisodes { get; init; }

	[JsonPropertyName("anime_num_episodes")]
	public int TotalEpisodes { get; init; }

	[JsonPropertyName("anime_airing_status")]
	public AnimeStatus AnimeAiringStatus { get; init; }

	[JsonPropertyName("anime_id")]
	public int Id { get; init; }

	GenericProgress IListEntry.UserProgress => this.UserAnimeProgress.ToGeneric();

	[JsonPropertyName("anime_title")]
	public string Title { get; init; } = null!;

	[JsonPropertyName("score")]
	public int Score { get; init; }

	[JsonConverter(typeof(JsonNumberToStringConverter))]
	[JsonPropertyName("tags")]
	public string Tags { get; init; } = null!;

	[JsonPropertyName("is_rewatching")]
	[JsonConverter(typeof(JsonToBoolConverter))]
	public bool IsRewatching { get; init; }

	int IListEntry.ProgressedSubEntries => this.WatchedEpisodes;

	int IListEntry.TotalSubEntries => this.TotalEpisodes;

	GenericStatus IListEntry.Status => this.AnimeAiringStatus.ToGeneric();

	bool IListEntry.IsReprogressing => this.IsRewatching;

	[JsonPropertyName("anime_url")]
	public string Url
	{
		get => this._url;
		init => this._url = Constants.BASE_URL + value;
	}

	[JsonPropertyName("anime_image_path")]
	public string ImageUrl
	{
		get => this._imageUrl;
		init => this._imageUrl = value.ToLargeImage();
	}

	[JsonPropertyName("anime_media_type_string")]
	public string MediaType { get; init; } = null!;
}