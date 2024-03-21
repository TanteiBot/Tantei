// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using PaperMalKing.Common.Attributes;

namespace PaperMalKing.Database.Models.AniList;

[Flags]
public enum AniListUserFeatures : ulong
{
	None = 0,

	[EnumDescription("animelist", "Track changes in AnimeList")]
	AnimeList = 1,

	[EnumDescription("mangalist", "Track changes in MangaList")]
	MangaList = 1 << 1,

	[EnumDescription("favourites", "Track changes in favourites")]
	Favourites = 1 << 2,

	[EnumDescription("mention", "Mention user in update")]
	Mention = 1 << 3,

	[EnumDescription("website", "show name and icon of website in update")]
	Website = 1 << 4,

	[EnumDescription("media format", "Show format of media in update (TV, Movie, Manga etc)")]
	MediaFormat = 1 << 5,

	[EnumDescription("media status", "Show status of media in update (Releasing, Finished etc)")]
	MediaStatus = 1 << 6,

	[EnumDescription("description", "Show description of anime and manga")]
	MediaDescription = 1 << 7,

	[EnumDescription("genres", "Show genres of anime and manga")]
	Genres = 1 << 8,

	[EnumDescription("tags", "Show tags of anime and manga")]
	Tags = 1 << 9,

	[EnumDescription("studio", "Show studio that made anime")]
	Studio = 1 << 10,

	[EnumDescription("mangaka", "Show authors of manga")]
	Mangaka = 1 << 11,

	[EnumDescription("reviews", "Track user reviews")]
	Reviews = 1 << 12,

	[EnumDescription("custom lists", "Show to which custom lists entry belongs to")]
	CustomLists = 1 << 13,

	[EnumDescription("director", "Show director of anime")]
	Director = 1 << 14,

	[EnumDescription("seyu", "Show seyu of anime")]
	Seyu = 1 << 15,
}