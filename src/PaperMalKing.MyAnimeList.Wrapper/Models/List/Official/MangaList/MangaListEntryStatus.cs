// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using System.Text.Json.Serialization;
using PaperMalKing.MyAnimeList.Wrapper.Models.List.Official.Base;

namespace PaperMalKing.MyAnimeList.Wrapper.Models.List.Official.MangaList;

internal sealed class MangaListEntryStatus : BaseListEntryStatus<MangaListStatus>
{
	public override ulong ProgressedSubEntries { get; }

	public override bool IsReprogressing => this.IsRereading;

	public override ulong ReprogressTimes => this.TimesReread;

	[JsonPropertyName("num_volumes_read")]
	public required ulong VolumesRead { get; init; }

	[JsonPropertyName("num_chapters_read")]
	public required ulong ChaptersRead { get; init; }

	[JsonPropertyName("is_rereading")]
	public required bool IsRereading { get; init; }

	[JsonPropertyName("num_times_reread")]
	public required ulong TimesReread { get; init; }
}