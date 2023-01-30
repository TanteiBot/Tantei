// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using System.Text.Json.Serialization;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.Base;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.MangaList;

public sealed class MangaListEntryStatus : BaseListEntryStatus<MangaListStatus>
{
	public override ulong ProgressedSubEntries { get; }

	public override bool IsReprogressing => this.IsRereading;

	public override ulong ReprogressTimes => this.TimesReread;

	[JsonPropertyName("num_volumes_read")]
	[JsonRequired]
	public ulong VolumesRead { get; internal set; }

	[JsonPropertyName("num_chapters_read")]
	[JsonRequired]
	public ulong ChaptersRead { get; internal set; }

	[JsonPropertyName("is_rereading")]
	[JsonRequired]
	public bool IsRereading { get; internal set; }

	[JsonPropertyName("num_times_reread")]
	[JsonRequired]
	public ulong TimesReread { get; internal set; }
}