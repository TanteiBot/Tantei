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

namespace Tantei.Core.Models.Users.AniList;

[Flags]
public enum AniListUserFeatures : ulong
{
	None = 0,

	[FeatureDescription("animelist", "track changes in animelist")]
	AnimeList = 1,

	[FeatureDescription("mangalist", "track changes in mangalist")]
	MangaList = 1 << 1,

	[FeatureDescription("favourites", "track changes in favourites")]
	Favourites = 1 << 2,

	[FeatureDescription("mention", "mention user in update")]
	Mention = 1 << 3,

	[FeatureDescription("website", "show name and icon of website in update")]
	Website = 1 << 4,

	[FeatureDescription("format", "show format of media in update (tv, movie, manga etc)")]
	MediaFormat = 1 << 5,

	[FeatureDescription("status", "show status of media in update (ongoing, finished etc)")]
	MediaStatus = 1 << 6,

	[FeatureDescription("description", "show description of anime and manga")]
	MediaDescription = 1 << 7,

	[FeatureDescription("genres", "show genres of anime and manga")]
	Genres = 1 << 8,

	[FeatureDescription("tags", "show tags of anime and manga")]
	Tags = 1 << 9,

	[FeatureDescription("studio", "show studio that made anime")]
	Studio = 1 << 10,

	[FeatureDescription("mangaka", "show mangaka that made manga")]
	Mangaka = 1 << 11,

	[FeatureDescription("reviews", "track user reviews")]
	Reviews = 1 << 12,

	[FeatureDescription("customlists", "show to which custom lists entry belongs to")]
	CustomLists = 1 << 13
}