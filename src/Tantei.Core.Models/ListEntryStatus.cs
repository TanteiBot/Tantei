// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

namespace Tantei.Core.Models;

public enum ListEntryStatus : byte
{
	Unknown = 0,
	Completed = 1,
	InProgress = 2,
	Dropped = 3,
	Paused = 4,
	Planning = 5
}