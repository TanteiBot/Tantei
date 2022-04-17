// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using System.ComponentModel;

namespace Tantei.Core.Models.Medias;

public enum Status : byte
{
	Unknown = 0,

	[Description("On-going")]
	Ongoing = 1,
	Hiatus = 2,
	Cancelled = 3,
	Finished = 4,

	[Description("Not Yet Started")]
	NotYetStarted = 5
}