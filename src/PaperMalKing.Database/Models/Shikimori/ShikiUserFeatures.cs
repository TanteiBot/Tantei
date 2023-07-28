// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using System.Diagnostics.CodeAnalysis;
using PaperMalKing.Common.Attributes;

namespace PaperMalKing.Database.Models.Shikimori;

[Flags]
[SuppressMessage("Roslynator", "RCS1154:Sort enum members.")]
public enum ShikiUserFeatures : ulong
{
	None = 0,

	Default = AnimeList | MangaList | Favourites | Mention | Website | MediaFormat | MediaStatus | Achievements,

	[FeatureDescription("animelist", "Track changes in AnimeList")]
	AnimeList = 1,

	[FeatureDescription("mangalist", "Track changes in MangaList")]
	MangaList = 1 << 1,

	[FeatureDescription("favorites", "Track changes in favorites")]
	Favourites = 1 << 2,

	[FeatureDescription("mention", "Mention user in update")]
	Mention = 1 << 3,

	[FeatureDescription("website", "Show name and icon of website in update")]
	Website = 1 << 4,

	[FeatureDescription("media format", "Show format of media in update (Tv, Movie, Manga etc)")]
	MediaFormat = 1 << 5,

	[FeatureDescription("media status", "Show status of media in update (Ongoing, Released etc)")]
	MediaStatus = 1 << 6,

	[FeatureDescription("russian", "show favorites, anime, manga, ranobe titles in russian")]
	Russian = 1 << 7,

	[FeatureDescription("mangaka", "Show authors of manga")]
	Mangaka = 1 << 8,

	[FeatureDescription("director", "Show director of anime")]
	Director = 1 << 9,

	[FeatureDescription("genres", "Show genres of media")]
	Genres = 1 << 10,

	[FeatureDescription("description", "Show truncated description of media")]
	Description = 1 << 11,

	[FeatureDescription("studios", "Show studios that made anime")]
	Studio = 1 << 12,

	[FeatureDescription("publisher", "Show publisher of manga")]
	Publisher = 1 << 13,

	[FeatureDescription("achievements", "Track user achievements")]
	Achievements = 1 << 14,
}