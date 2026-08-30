// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Collections.ObjectModel;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Search;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Search;

internal sealed class PickerSnapshot
{
	public const int MaximumResults = 100;

	public ReadOnlyCollection<PickerEntry> Entries { get; }

	public int PageCount => (this.Entries.Count + PickerRenderer.PageSize - 1) / PickerRenderer.PageSize;

	private PickerSnapshot(IEnumerable<PickerEntry> entries)
	{
		var snapshot = entries.Take(MaximumResults).ToArray();
		if (snapshot.Length == 0)
		{
			throw new ArgumentException("A Picker requires at least one Search Result.", nameof(entries));
		}

		this.Entries = Array.AsReadOnly(snapshot);
	}

	public static PickerSnapshot ForAnime(IEnumerable<RankedSearchResult<AnimeSearchResult>> results) => new(results.Select(PickerEntry.ForAnime));

	public static PickerSnapshot ForManga(IEnumerable<RankedSearchResult<MangaSearchResult>> results) => new(results.Select(PickerEntry.ForManga));
}
