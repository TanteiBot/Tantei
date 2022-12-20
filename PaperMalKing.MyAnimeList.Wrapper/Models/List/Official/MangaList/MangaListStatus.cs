// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using System.ComponentModel;

namespace PaperMalKing.MyAnimeList.Wrapper.Models.List.Official.MangaList;

internal enum MangaListStatus : byte
{
	unknown = 0,
	[Description("Reading")]
	reading = 1,
	[Description("Completed")]
	completed = 2,
	[Description("On-hold")]
	on_hold = 3,
	[Description("Dropped")]
	dropped = 4,
	[Description("Plan to read")]
	plan_to_read = 5
}