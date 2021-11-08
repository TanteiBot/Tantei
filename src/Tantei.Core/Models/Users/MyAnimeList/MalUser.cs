// Tantei.
// Copyright (C) 2021 N0D4N
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

namespace Tantei.Core.Models.Users.MyAnimeList;

public sealed record MalUser(BotUser BotUser, ulong BotUserId, ulong Id, string Username, DateTimeOffset LastUpdatedAnimeTimeStamp,
							 DateTimeOffset LastUpdatedMangaTimeStamp, string LastAnimeUpdateHash, string LastMangaUpdateHash,
							 MalUserFeatures Features)
{
	public IList<MalUserFavorite> FavoriteAnime { get; init; } = Array.Empty<MalUserFavorite>();
}