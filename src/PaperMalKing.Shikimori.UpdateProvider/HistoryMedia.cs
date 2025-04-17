// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2025 N0D4N

using PaperMalKing.Shikimori.Wrapper.Abstractions.Models;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Media;

namespace PaperMalKing.Shikimori.UpdateProvider;

internal sealed record HistoryMedia(List<History> HistoryEntries)
{
	public BaseMedia? Media { get; set; }

	public uint MinId => this.HistoryEntries is null or [] ? 0 : this.HistoryEntries.Min(static h => h.Id);

	public uint MaxId => this.HistoryEntries is null or [] ? 0 : this.HistoryEntries.Max(static h => h.Id);
}