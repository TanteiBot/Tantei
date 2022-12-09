// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System;
using PaperMalKing.Common.Attributes;

namespace PaperMalKing.Database.Models.Shikimori;

[Flags]
public enum ShikiUserFeatures : ulong
{
	None = 0,

	[FeatureDescription("animelist", "track changes in animelist")]
	AnimeList = 1,

	[FeatureDescription("mangalist", "track changes in mangalist")]
	MangaList = 1 << 1,

	[FeatureDescription("favorites", "track changes in favorites")]
	Favourites = 1 << 2,

	[FeatureDescription("mention", "mention user in update")]
	Mention = 1 << 3,

	[FeatureDescription("website", "show name and icon of website in update")]
	Website = 1 << 4,

	[FeatureDescription("mediaformat", "show format of media in update (tv, movie, manga etc)")]
	MediaFormat = 1 << 5,

	[FeatureDescription("mediastatus", "show status of media in update (ongoing, finished etc)")]
	MediaStatus = 1 << 6,
		
	[FeatureDescription("russian", "show favorites, anime, manga, ranobe titles in russian")]
	Russian = 1 << 7
}