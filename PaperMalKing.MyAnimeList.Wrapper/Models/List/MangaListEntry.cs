#region LICENSE
// PaperMalKing.
// Copyright (C) 2021-2022 N0D4N
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

namespace PaperMalKing.MyAnimeList.Wrapper.Models.List
{
	internal sealed class MangaListEntry : IListEntry
	{
		private readonly string _url = null!;
		private readonly MangaProgress _userMangaProgress;
		private readonly string _imageUrl = null!;

		[JsonPropertyName("status")]
		public MangaProgress UserMangaProgress
		{
			get => this.IsRereading ? MangaProgress.Rereading : this._userMangaProgress;
			init => this._userMangaProgress = value;
		}

		[JsonPropertyName("num_read_chapters")]
		public int ReadChapters { get; init; }

		[JsonPropertyName("num_read_volumes")]
		public int ReadVolumes { get; init; }

		[JsonPropertyName("manga_num_chapters")]
		public int TotalChapters { get; init; }

		[JsonPropertyName("manga_num_volumes")]
		public int TotalVolumes { get; init; }

		[JsonPropertyName("manga_publishing_status")]
		public MangaStatus MangaPublishingStatus { get; init; }

		bool IListEntry.IsReprogressing => this.IsRereading;

		[JsonPropertyName("is_rereading")]
		[JsonConverter(typeof(JsonToBoolConverter))]
		public bool IsRereading { get; init; }

		[JsonPropertyName("manga_id")]
		public int Id { get; init; }

		GenericProgress IListEntry.UserProgress => this.UserMangaProgress.ToGeneric();

		[JsonPropertyName("manga_title")]
		public string Title { get; init; } = null!;

		[JsonPropertyName("score")]
		public int Score { get; init; }

		[JsonConverter(typeof(JsonNumberToStringConverter))]
		[JsonPropertyName("tags")]
		public string Tags { get; init; } = null!;

		int IListEntry.ProgressedSubEntries => this.ReadChapters;

		int IListEntry.TotalSubEntries => this.TotalChapters;

		GenericStatus IListEntry.Status => this.MangaPublishingStatus.ToGeneric();

		[JsonPropertyName("manga_url")]
		public string Url
		{
			get => this._url;
			init => this._url = Constants.BASE_URL + value;
		}

		[JsonPropertyName("manga_image_path")]
		public string ImageUrl
		{
			get => this._imageUrl;
			init => this._imageUrl = value.ToLargeImage();
		}

		[JsonPropertyName("manga_media_type_string")]
		public string MediaType { get; init; } = null!;
	}
}