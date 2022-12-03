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

namespace PaperMalKing.MyAnimeList.Wrapper.Models.Favorites
{
	internal class BaseListFavorite : BaseFavorite
	{
		internal string Type { get; init; }

		internal int StartYear { get; init; }

		internal BaseListFavorite(string type, int startYear, MalUrl url, string name, string? imageUrl) : base(url,
			name, imageUrl)
		{
			this.Type = type;
			this.StartYear = startYear;
		}

		internal BaseListFavorite(string type, int startYear, BaseFavorite baseFav) : this(type, startYear, baseFav.Url,
			baseFav.Name, baseFav.ImageUrl)
		{ }

		internal BaseListFavorite(BaseListFavorite other) : this(other.Type, other.StartYear, other.Url, other.Name,
			other.ImageUrl)
		{ }
	}
}