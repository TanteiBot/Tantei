// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.Text.Json.Serialization;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.AnimeList;

[JsonConverter(typeof(JsonStringEnumConverter<AnimeAiringStatus>))]
public enum AnimeAiringStatus : byte
{
	[JsonStringEnumMemberName("uknown")]
	Unknown = 0,

	[JsonStringEnumMemberName("finished_airing")]
	FinishedAiring = 1,

	[JsonStringEnumMemberName("currently_airing")]
	CurrentlyAiring = 2,

	[JsonStringEnumMemberName("not_yet_aired")]
	NotYetAired = 3,
}