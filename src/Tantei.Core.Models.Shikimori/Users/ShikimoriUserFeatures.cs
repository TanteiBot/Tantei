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

namespace Tantei.Core.Models.Shikimori.Users;

[Flags]
public enum ShikimoriUserFeatures : ulong
{
	None = 0,

	[FeatureDescription("animelist", "track changes in animelist")]
	AnimeList = 1,

	[FeatureDescription("mangalist", "track changes in mangalist")]
	MangaList = 1 << 1,

	[FeatureDescription("favorites", "track changes in favorites")]
	Favourites = 1 << 2,

	[FeatureDescription("mention", "mention user in update")]
	Mention = 1 << 3,

	[FeatureDescription("website", "show name and icon of website in update")]
	Website = 1 << 4,

	[FeatureDescription("mediaformat", "show format of media in update (tv, movie, manga etc)")]
	MediaFormat = 1 << 5,

	[FeatureDescription("mediastatus", "show status of media in update (ongoing, finished etc)")]
	MediaStatus = 1 << 6,

	[FeatureDescription("russian", "show favorites, anime, manga, ranobe titles in russian")]
	Russian = 1 << 7
}