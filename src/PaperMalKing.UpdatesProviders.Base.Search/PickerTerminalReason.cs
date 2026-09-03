// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

namespace PaperMalKing.UpdatesProviders.Base.Search;

internal enum PickerTerminalReason
{
	Picked = 0,
	Cancelled = 1,
	InactivityTimeout = 2,
	AbsoluteTimeout = 3,
	PostFailed = 4,
	InteractionFailed = 5,
}
