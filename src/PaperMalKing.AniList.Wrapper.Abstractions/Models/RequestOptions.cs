// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;

namespace PaperMalKing.AniList.Wrapper.Abstractions.Models;

[Flags]
public enum RequestOptions : ulong
{
	AnimeList = 1 << 0,
	MangaList = 1 << 1,
	Favourites = 1 << 2,
	MediaFormat = 1 << 5,
	MediaStatus = 1 << 6,
	MediaDescription = 1 << 7,
	Genres = 1 << 8,
	Tags = 1 << 9,
	Studio = 1 << 10,
	Mangaka = 1 << 11,
	Reviews = 1 << 12,
	CustomLists = 1 << 13,
	Director = 1 << 14,
	Seyu = 1 << 15,

	All = AnimeList | MangaList | Favourites | MediaFormat | MediaStatus | MediaDescription | Genres | Tags | Studio | Mangaka | Reviews |
		  CustomLists | Director | Seyu
}