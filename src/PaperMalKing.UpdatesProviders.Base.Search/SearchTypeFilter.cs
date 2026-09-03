// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

namespace PaperMalKing.UpdatesProviders.Base.Search;

internal readonly record struct SearchTypeFilter(Enum Value, string Label)
{
	public static SearchTypeFilter? From<TEnum>(TEnum? value)
		where TEnum : struct, Enum => value is { } resolved ? new SearchTypeFilter(resolved, resolved.ToString()) : null;

	public TEnum As<TEnum>()
		where TEnum : struct, Enum => (TEnum)this.Value;
}
