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

using PaperMalKing.MyAnimeList.Wrapper.Models.Favorites;

namespace PaperMalKing.MyAnimeList.Wrapper.Models;

internal sealed class User
{
	private string? _avatarUrl;
	private string? _profileUrl;
	internal string Username { get; init; } = null!;

	internal string ProfileUrl => this._profileUrl ??= $"{Constants.PROFILE_URL}{this.Username}";

	internal string AvatarUrl =>
		this._avatarUrl ??=
			$"{Constants.USER_AVATAR}{this.Id.ToString()}.jpg?t={DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()}";

	internal int Id { get; init; }

	internal bool HasPublicAnimeUpdates => this.LatestAnimeUpdate is not null;

	internal bool HasPublicMangaUpdates => this.LatestMangaUpdate is not null;

	internal LatestInProfileUpdate? LatestMangaUpdate { get; init; }

	internal LatestInProfileUpdate? LatestAnimeUpdate { get; init; }

	internal UserFavorites Favorites { get; init; } = null!;
}