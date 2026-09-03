// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Globalization;

namespace PaperMalKing.UpdatesProviders.Base.Search;

internal static class SearchPresentation
{
	private const uint Thousand = 1_000U;
	private const uint Million = 1_000_000U;
	private const string Separator = " · ";

	public static string ComposeOptionDescription(IEnumerable<string?> parts)
	{
		ArgumentNullException.ThrowIfNull(parts);
		return string.Join(Separator, parts.Where(static part => !string.IsNullOrWhiteSpace(part)));
	}

	public static string AbbreviateCount(uint count) => count switch
	{
		>= Million => $"{(count / (double)Million).ToString("0.#", CultureInfo.InvariantCulture)}M",
		>= Thousand => $"{(count / (double)Thousand).ToString("0.#", CultureInfo.InvariantCulture)}K",
		_ => count.ToString(CultureInfo.InvariantCulture),
	};
}
