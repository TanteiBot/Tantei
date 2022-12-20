// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using System.ComponentModel;

namespace PaperMalKing.MyAnimeList.Wrapper.Models.List.Official.AnimeList;

internal enum AnimeMediaType : byte
{
	unknown = 0,
	[Description("TV")]
	tv = 1,
	[Description("OVA")]
	ova = 2,
	[Description("Movie")]
	movie = 3,
	[Description("Special")]
	special = 4,
	[Description("ONA")]
	ona = 5,
	[Description("Music")]
	music = 6
}