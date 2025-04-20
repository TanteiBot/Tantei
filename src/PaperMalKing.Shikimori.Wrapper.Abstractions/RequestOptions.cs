// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2025 N0D4N

namespace PaperMalKing.Shikimori.Wrapper.Abstractions;

[Flags]
public enum RequestOptions : ulong
{
	None = 0,

	AnimeList = 1,

	MangaList = 1 << 1,

	Favourites = 1 << 2,

	MediaFormat = 1 << 5,

	MediaStatus = 1 << 6,

	Russian = 1 << 7,

	Mangaka = 1 << 8,

	Director = 1 << 9,

	Genres = 1 << 10,

	Description = 1 << 11,

	Studio = 1 << 12,

	Publisher = 1 << 13,
}