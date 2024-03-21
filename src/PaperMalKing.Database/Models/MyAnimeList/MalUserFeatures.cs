// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using PaperMalKing.Common.Attributes;

namespace PaperMalKing.Database.Models.MyAnimeList;

[Flags]
public enum MalUserFeatures : ulong
{
	None = 0,

	[EnumDescription("animelist", "Track changes in AnimeList")]
	AnimeList = 1,

	[EnumDescription("mangalist", "Track changes in MangaList")]
	MangaList = 1 << 1,

	[EnumDescription("favorites", "Track changes in favorites")]
	Favorites = 1 << 2,

	[EnumDescription("mention", "Mention user in update")]
	Mention = 1 << 3,

	[EnumDescription("website", "Show name and icon of website in update")]
	Website = 1 << 4,

	[EnumDescription("media format", "Show format of media in update (TV, Movie, Manga etc)")]
	MediaFormat = 1 << 5,

	[EnumDescription("media status", "show status of media in update (Currently airing, Finished airing etc)")]
	MediaStatus = 1 << 6,

	[EnumDescription("genres", "Show genres of media in update")]
	Genres = 1 << 7,

	[EnumDescription("synopsis", "Show synopsis of media in update")]
	Synopsis = 1 << 8,

	[EnumDescription("studios", "Show studios that made anime in update")]
	Studio = 1 << 9,

	[EnumDescription("mangakas", "Show mangakas that made manga in update")]
	Mangakas = 1 << 10,

	[EnumDescription("tags", "Show tags of list entry in update")]
	Tags = 1 << 11,

	[EnumDescription("comments", "Show comments of list entry in update")]
	Comments = 1 << 12,

	[EnumDescription("dates", "Show start and finish dates of list entry")]
	Dates = 1 << 13,

	[EnumDescription("themes", "Show themes of media in update")]
	Themes = 1 << 14,

	[EnumDescription("demographic", "Show demographic of media in update")]
	Demographic = 1 << 15,

	[EnumDescription("seiyu", "Show seiyu of anime in update")]
	Seiyu = 1 << 16,
}