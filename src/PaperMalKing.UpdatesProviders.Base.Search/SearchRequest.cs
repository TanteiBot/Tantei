// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Runtime.InteropServices;

namespace PaperMalKing.UpdatesProviders.Base.Search;

[StructLayout(LayoutKind.Auto)]
internal readonly record struct SearchRequest(
	MatchKey QueryKey,
	string RawQuery,
	PickerMediaKind MediaKind,
	SearchTypeFilter? Filter)
{
	public ulong RequesterId { get; init; }

	public bool IncludeNsfw { get; init; }
}
