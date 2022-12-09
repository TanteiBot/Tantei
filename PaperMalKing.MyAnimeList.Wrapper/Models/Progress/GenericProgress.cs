// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System.ComponentModel;

namespace PaperMalKing.MyAnimeList.Wrapper.Models.Progress
{
	internal enum GenericProgress : byte
	{
		Unknown = 0,

		CurrentlyInProgress = 1,

		Completed = 2,

		OnHold = 3,

		Dropped = 4,

		InPlans = 6,

		[Description("Re-progressing")]
		Reprogressing = 7,

		All = byte.MaxValue
	}
}