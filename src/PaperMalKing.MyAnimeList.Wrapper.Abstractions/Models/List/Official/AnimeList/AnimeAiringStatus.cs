// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using System.ComponentModel;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.AnimeList;

public enum AnimeAiringStatus : byte
{
	uknown = 0,
	[Description("Finished airing")]
	finished_airing = 1,
	[Description("Currently airing")]
	currently_airing = 2,
	[Description("Not yet aired")]
	not_yet_aired = 3
}