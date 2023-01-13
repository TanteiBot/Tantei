// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System;
using PaperMalKing.Common.Attributes;

namespace PaperMalKing.Database.Models.AniList;

[Flags]
public enum AniListUserFeatures : ulong
{
	None = 0,

	[FeatureDescription("animelist", "Track changes in AnimeList")]
	AnimeList = 1,

	[FeatureDescription("mangalist", "Track changes in MangaList")]
	MangaList = 1 << 1,

	[FeatureDescription("favourites", "Track changes in favourites")]
	Favourites = 1 << 2,

	[FeatureDescription("mention", "Mention user in update")]
	Mention = 1 << 3,

	[FeatureDescription("website", "show name and icon of website in update")]
	Website = 1 << 4,

	[FeatureDescription("media format", "Show format of media in update (TV, Movie, Manga etc)")]
	MediaFormat = 1 << 5,

	[FeatureDescription("media status", "Show status of media in update (Releasing, Finished etc)")]
	MediaStatus = 1 << 6,

	[FeatureDescription("description", "Show description of anime and manga")]
	MediaDescription = 1 << 7,

	[FeatureDescription("genres", "Show genres of anime and manga")]
	Genres = 1 << 8,

	[FeatureDescription("tags", "Show tags of anime and manga")]
	Tags = 1 << 9,

	[FeatureDescription("studio", "Show studio that made anime")]
	Studio = 1 << 10,

	[FeatureDescription("mangaka", "Show mangaka that made manga")]
	Mangaka = 1 << 11,

	[FeatureDescription("reviews", "Track user reviews")]
	Reviews = 1 << 12,

	[FeatureDescription("custom lists", "Show to which custom lists entry belongs to")]
	CustomLists = 1 << 13,

	[FeatureDescription("director", "Show director of anime")]
	Director = 1 << 14,
}