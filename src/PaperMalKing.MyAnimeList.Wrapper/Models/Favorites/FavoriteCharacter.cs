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

namespace PaperMalKing.MyAnimeList.Wrapper.Models.Favorites;

internal sealed class FavoriteCharacter : BaseFavorite
{
	internal MalUrl FromUrl { get; init; }

	internal string FromName { get; init; }

	internal FavoriteCharacter(MalUrl fromUrl, string fromName, MalUrl url, string name, string? imageUrl) : base(
		url, name, imageUrl)
	{
		this.FromUrl = fromUrl;
		this.FromName = fromName;
	}

	internal FavoriteCharacter(MalUrl fromUrl, string fromName, BaseFavorite baseFav) : this(fromUrl, fromName,
		baseFav.Url, baseFav.Name, baseFav.ImageUrl)
	{ }
}