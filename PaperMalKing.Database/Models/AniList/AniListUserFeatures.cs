// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System;
using PaperMalKing.Common.Attributes;

namespace PaperMalKing.Database.Models.AniList
{
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

		[FeatureDescription("mediaformat", "show format of media in update (tv, movie, manga etc)")]
		MediaFormat = 1 << 5,

		[FeatureDescription("mediastatus", "show status of media in update (ongoing, finished etc)")]
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
}