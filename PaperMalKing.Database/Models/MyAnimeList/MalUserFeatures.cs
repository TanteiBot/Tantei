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

using System;
using PaperMalKing.Common.Attributes;

namespace PaperMalKing.Database.Models.MyAnimeList
{
	[Flags]
	public enum MalUserFeatures : ulong
	{
		[FeatureDescription("animelist", "Track changes in animelist")]
		AnimeList = 1,
		[FeatureDescription("mangalist", "Track changes in mangalist")]
		MangaList = 2,
		[FeatureDescription("favorites", "Track changes in favorites")]
		Favorites = 4,
		[FeatureDescription("mention", "Mention user in update")]
		Mention = 8,
		[FeatureDescription("website", "Show name and icon of website in update")]
		Website = 16,
		[FeatureDescription("mediaformat", "Show format of media in update (tv, movie, manga etc)")]
		MediaFormat = 32,
		[FeatureDescription("mediastatus", "Show status of media in update (Ongoing, finished etc)")]
		MediaStatus = 64,
		Default = AnimeList | MangaList | Favorites | Mention | Website | MediaFormat | MediaStatus
	}
}