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

using Tantei.Shared;

namespace Tantei.Core.Models.Users.MyAnimeList;

public enum MalUserFeatures : ulong
{
	None = 0,

	[FeatureDescription("anime", "track changes in animelist")]
	Anime = 1 << 0,

	[FeatureDescription("manga", "track changes in mangalist")]
	Manga = 1 << 1,

	[FeatureDescription("favorites", "track changes in favorites")]
	Favorites = 1 << 2,

	[FeatureDescription("mention", "mention user in update")]
	Mention = 1 << 3,

	[FeatureDescription("website", "show name and icon of website in update")]
	Website = 1 << 4,

	[FeatureDescription("format", "show format of media in update (tv, movie, manga etc)")]
	MediaFormat = 1 << 5,

	[FeatureDescription("status", "show status of media in update (ongoing, finished etc)")]
	MediaStatus = 1 << 6,

	[FeatureDescription("studio", "show name of studio(s) which made an anime")]
	Studio = 1 << 7,

	[FeatureDescription("seiyu", "show name of seiyu(s) that voiced character in anime")]
	Seiyu = 1 << 8,

	[FeatureDescription("mangaka", "show name of mangaka that made a manga")]
	Mangaka = 1 << 9
}