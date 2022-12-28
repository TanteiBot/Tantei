// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using System.ComponentModel;

namespace PaperMalKing.MyAnimeList.Wrapper.Models.List.Official.MangaList;

internal enum MangaMediaType : byte
{
	unknown = 0,
	[Description("Manga")]
	manga = 1,
	[Description("Novel")]
	novel = 2,
	[Description("One-shot")]
	one_shot = 3,
	[Description("Doujinshi")]
	doujinshi = 4,
	[Description("Manhwa")]
	manhwa = 5,
	[Description("Manhua")]
	manhua = 6,
	[Description("Eol")]
	oel = 7,
	[Description("Light novel")]
	light_novel = 8
}