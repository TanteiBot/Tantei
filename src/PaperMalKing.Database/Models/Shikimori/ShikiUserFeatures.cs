// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using PaperMalKing.Common.Attributes;

namespace PaperMalKing.Database.Models.Shikimori;

[Flags]
public enum ShikiUserFeatures : ulong
{
	None = 0,

	[EnumDescription("animelist", "Track changes in AnimeList")]
	AnimeList = 1,

	[EnumDescription("mangalist", "Track changes in MangaList")]
	MangaList = 1 << 1,

	[EnumDescription("favorites", "Track changes in favorites")]
	Favourites = 1 << 2,

	[EnumDescription("mention", "Mention user in update")]
	Mention = 1 << 3,

	[EnumDescription("website", "Show name and icon of website in update")]
	Website = 1 << 4,

	[EnumDescription("media format", "Show format of media in update (Tv, Movie, Manga etc)")]
	MediaFormat = 1 << 5,

	[EnumDescription("media status", "Show status of media in update (Ongoing, Released etc)")]
	MediaStatus = 1 << 6,

	[EnumDescription("russian", "show favorites, anime, manga, ranobe titles in russian")]
	Russian = 1 << 7,

	[EnumDescription("mangaka", "Show authors of manga")]
	Mangaka = 1 << 8,

	[EnumDescription("director", "Show director of anime")]
	Director = 1 << 9,

	[EnumDescription("genres", "Show genres of media")]
	Genres = 1 << 10,

	[EnumDescription("description", "Show truncated description of media")]
	Description = 1 << 11,

	[EnumDescription("studios", "Show studios that made anime")]
	Studio = 1 << 12,

	[EnumDescription("publisher", "Show publisher of manga")]
	Publisher = 1 << 13,

	[EnumDescription("achievements", "Track user achievements")]
	Achievements = 1 << 14,
}