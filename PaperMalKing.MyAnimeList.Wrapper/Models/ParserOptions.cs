// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System;

namespace PaperMalKing.MyAnimeList.Wrapper.Models
{
	[Flags]
	internal enum ParserOptions : byte
	{
		None = 0,
		Favorites = 1,
		AnimeList = 1 << 1,
		MangaList = 1 << 2
	}
}