// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.MangaList;

[JsonConverter(typeof(JsonStringEnumConverter<MangaListStatus>))]
public enum MangaListStatus : byte
{
	[JsonStringEnumMemberName("unknown")]
	Unknown = 0,

	[JsonStringEnumMemberName("reading")]
	Reading = 1,

	[JsonStringEnumMemberName("completed")]
	Completed = 2,

	[Description("On-hold")]
	[JsonStringEnumMemberName("on_hold")]
	OnHold = 3,

	[JsonStringEnumMemberName("dropped")]
	Dropped = 4,

	[JsonStringEnumMemberName("plan_to_read")]
	PlanToRead = 5,
}