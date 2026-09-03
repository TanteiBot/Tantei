// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Globalization;

namespace PaperMalKing.UpdatesProviders.Base.Search;

internal static class SearchText
{
	private const string Ellipsis = "…";

	public static string Truncate(string value, int maximumLength)
	{
		ArgumentNullException.ThrowIfNull(value);
		ArgumentOutOfRangeException.ThrowIfLessThan(maximumLength, Ellipsis.Length);
		if (value.Length <= maximumLength)
		{
			return value;
		}

		var budget = maximumLength - Ellipsis.Length;
		var cutoff = 0;
		while (cutoff < budget)
		{
			var elementLength = StringInfo.GetNextTextElementLength(value.AsSpan(cutoff));
			if (cutoff + elementLength > budget)
			{
				break;
			}

			cutoff += elementLength;
		}

		return value[..cutoff] + Ellipsis;
	}
}
