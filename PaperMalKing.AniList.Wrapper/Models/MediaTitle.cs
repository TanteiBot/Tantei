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

using System.Collections.Generic;
using System.Text.Json.Serialization;
using PaperMalKing.AniList.Wrapper.Models.Enums;

namespace PaperMalKing.AniList.Wrapper.Models
{
	public sealed class MediaTitle
	{
		private readonly Dictionary<TitleLanguage, string?> _titles = new(6);

		[JsonPropertyName("stylisedRomaji")]
		public string? StylisedRomaji
		{
			get => this._titles[TitleLanguage.ROMAJI_STYLISED];
			init => this._titles.Add(TitleLanguage.ROMAJI_STYLISED, value);
		}

		[JsonPropertyName("romaji")]
		public string? Romaji
		{
			get => this._titles[TitleLanguage.ROMAJI];
			init => this._titles.Add(TitleLanguage.ROMAJI, value);
		}

		[JsonPropertyName("stylisedEnglish")]
		public string? StylisedEnglish
		{
			get => this._titles[TitleLanguage.ENGLISH_STYLISED];
			init => this._titles.Add(TitleLanguage.ENGLISH_STYLISED, value);
		}

		[JsonPropertyName("english")]
		public string? English
		{
			get => this._titles[TitleLanguage.ENGLISH];
			init => this._titles.Add(TitleLanguage.ENGLISH, value);
		}

		[JsonPropertyName("stylisedNative")]
		public string? StylisedNative
		{
			get => this._titles[TitleLanguage.NATIVE_STYLISED];
			init => this._titles.Add(TitleLanguage.NATIVE_STYLISED, value);
		}

		[JsonPropertyName("native")]
		public string? Native
		{
			get => this._titles[TitleLanguage.NATIVE];
			init => this._titles.Add(TitleLanguage.NATIVE, value);
		}

		public string GetTitle(TitleLanguage titleLanguage)
		{
			var title = "";
			for (; titleLanguage != TitleLanguage.NATIVE - 1; titleLanguage--)
			{
				title = this._titles[titleLanguage];
				if (!string.IsNullOrEmpty(title)) break;
			}

			return title!;
		}
	}
}