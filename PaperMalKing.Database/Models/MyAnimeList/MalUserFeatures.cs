// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System;
using PaperMalKing.Common.Attributes;

namespace PaperMalKing.Database.Models.MyAnimeList;

[Flags]
public enum MalUserFeatures : ulong
{
	None = 0,
	[FeatureDescription("animelist", "Track changes in AnimeList")]
	AnimeList = 1,
	[FeatureDescription("mangalist", "Track changes in MangaList")]
	MangaList = 1 << 1,
	[FeatureDescription("favorites", "Track changes in favorites")]
	Favorites = 1 << 2,
	[FeatureDescription("mention", "Mention user in update")]
	Mention = 1 << 3,
	[FeatureDescription("website", "Show name and icon of website in update")]
	Website = 1 << 4,
	[FeatureDescription("media format", "Show format of media in update (TV, Movie, Manga etc)")]
	MediaFormat = 1 << 5,
	[FeatureDescription("media status", "show status of media in update (Currently airing, Finished airing etc)")]
	MediaStatus = 1 << 6,
	[FeatureDescription("genres", "Show genres of media in update")]
	Genres = 1 << 7,
	[FeatureDescription("synopsis", "Show synopsis of media in update")]
	Synopsis = 1 << 8,
	[FeatureDescription("studios", "Show studios that made anime in update")]
	Studio = 1 << 9,
	[FeatureDescription("mangakas", "Show mangakas that made manga in update")]
	Mangakas = 1 << 10,
	[FeatureDescription("tags", "Show tags of list entry in update")]
	Tags = 1 << 11,
	[FeatureDescription("comments", "Show comments of list entry in update")]
	Comments = 1 << 12
}