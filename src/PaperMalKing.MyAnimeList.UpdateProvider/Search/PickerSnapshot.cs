// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Collections.ObjectModel;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Search;

internal sealed class PickerSnapshot
{
	public const int MaximumResults = 100;

	public ReadOnlyCollection<SearchResult> Results { get; }

	public int PageCount => (this.Results.Count + PickerRenderer.PageSize - 1) / PickerRenderer.PageSize;

	private PickerSnapshot(IEnumerable<SearchResult> results)
	{
		var snapshot = results.Take(MaximumResults).ToArray();
		if (snapshot.Length == 0)
		{
			throw new ArgumentException("A Picker requires at least one Search Result.", nameof(results));
		}

		this.Results = Array.AsReadOnly(snapshot);
	}

	public static PickerSnapshot Create(IEnumerable<SearchResult> results) => new(results);
}
