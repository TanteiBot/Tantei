// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System;
using PaperMalKing.Common.Attributes;

namespace PaperMalKing.Database.Models.Shikimori;

[Flags]
public enum ShikiUserFeatures : ulong
{
	None = 0,

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
	Russian = 1 << 7
}