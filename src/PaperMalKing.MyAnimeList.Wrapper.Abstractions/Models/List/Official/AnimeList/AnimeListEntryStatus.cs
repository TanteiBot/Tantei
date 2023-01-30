// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using System.Text.Json.Serialization;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.Base;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.AnimeList;

public sealed class AnimeListEntryStatus : BaseListEntryStatus<AnimeListStatus>
{
	public override ulong ProgressedSubEntries => this.EpisodesWatched;

	public override bool IsReprogressing => this.IsRewatching;

	public override ulong ReprogressTimes => this.TimesRewatched;


	[JsonPropertyName("num_episodes_watched")]
	[JsonRequired]
	public ulong EpisodesWatched { get; internal set; }

	[JsonPropertyName("is_rewatching")]
	[JsonRequired]
	public bool IsRewatching { get; internal set; }

	[JsonPropertyName("num_times_rewatched")]
	[JsonRequired]
	public ulong TimesRewatched { get; internal set; }
}