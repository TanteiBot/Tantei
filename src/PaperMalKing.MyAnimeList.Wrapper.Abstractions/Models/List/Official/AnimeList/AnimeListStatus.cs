// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.AnimeList;

[JsonConverter(typeof(JsonStringEnumConverter<AnimeListStatus>))]
public enum AnimeListStatus : byte
{
	[JsonStringEnumMemberName("unknown")]
	Unknown = 0,

	[JsonStringEnumMemberName("watching")]
	Watching = 1,

	[JsonStringEnumMemberName("completed")]
	Completed = 2,

	[JsonStringEnumMemberName("on_hold")]
	[Description("On-hold")]
	OnHold = 3,

	[JsonStringEnumMemberName("dropped")]
	Dropped = 4,

	[JsonStringEnumMemberName("plan_to_watch")]
	PlanToWatch = 5,
}