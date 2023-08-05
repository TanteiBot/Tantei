// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N
using System.Text.Json.Serialization;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.Base;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.AnimeList;

public sealed class AnimeListEntryStatus : BaseListEntryStatus<AnimeListStatus>
{
	public override ulong ProgressedSubEntries => this.EpisodesWatched;

	public override bool IsReprogressing => this.IsRewatching;

	public override ulong ReprogressTimes => this.TimesRewatched;

	[JsonPropertyName("num_episodes_watched")]
	public required ulong EpisodesWatched { get; init; }

	[JsonPropertyName("is_rewatching")]
	public required bool IsRewatching { get; init; }

	[JsonPropertyName("num_times_rewatched")]
	public required ulong TimesRewatched { get; init; }
}