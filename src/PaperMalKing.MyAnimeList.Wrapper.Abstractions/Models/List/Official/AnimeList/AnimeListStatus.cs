// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.AnimeList;

[JsonConverter(typeof(JsonStringEnumConverter<AnimeListStatus>))]
public enum AnimeListStatus : byte
{
	unknown = 0,
	[Description("Watching")]
	watching = 1,
	[Description("Completed")]
	completed = 2,
	[Description("On-hold")]
	on_hold = 3,
	[Description("Dropped")]
	dropped = 4,
	[Description("Plan to watch")]
	plan_to_watch = 5,
}