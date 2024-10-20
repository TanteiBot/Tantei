// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<MediaListStatus>))]
public enum MediaListStatus : byte
{
	[JsonStringEnumMemberName("CURRENT")]
	Current = 0,

	[JsonStringEnumMemberName("PLANNING")]
	Planning = 1,

	[JsonStringEnumMemberName("COMPLETED")]
	Completed = 2,

	[JsonStringEnumMemberName("DROPPED")]
	Dropped = 3,

	[JsonStringEnumMemberName("PAUSED")]
	Paused = 4,

	[JsonStringEnumMemberName("REPEATING")]
	Repeating = 5,
}