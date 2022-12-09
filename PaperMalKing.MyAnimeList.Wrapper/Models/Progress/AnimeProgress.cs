// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System.ComponentModel;

namespace PaperMalKing.MyAnimeList.Wrapper.Models.Progress;

internal enum AnimeProgress : byte
{
	Unknown = 0,

	Watching = 1,

	Completed = 2,

	OnHold = 3,

	Dropped = 4,

	PlanToWatch = 6,

	[Description("Re-watching")]
	Rewatching = 7,

	All = byte.MaxValue
}