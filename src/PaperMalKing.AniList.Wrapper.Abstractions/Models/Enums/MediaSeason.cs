// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<MediaSeason>))]
public enum MediaSeason : byte
{
	[JsonStringEnumMemberName("WINTER")]
	Winter = 0,

	[JsonStringEnumMemberName("SPRING")]
	Spring = 1,

	[JsonStringEnumMemberName("SUMMER")]
	Summer = 2,

	[JsonStringEnumMemberName("FALL")]
	Fall = 3,
}
