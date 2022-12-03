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

using System;

namespace PaperMalKing.AniList.Wrapper.Models
{
	[Flags]
	public enum RequestOptions : ulong
	{
		AnimeList = 1        << 0,
		MangaList = 1        << 1,
		Favourites = 1       << 2,
		MediaFormat = 1      << 5,
		MediaStatus = 1      << 6,
		MediaDescription = 1 << 7,
		Genres = 1           << 8,
		Tags = 1             << 9,
		Studio = 1           << 10,
		Mangaka = 1          << 11,
		Reviews = 1          << 12,
		CustomLists = 1		 << 13
	}
}