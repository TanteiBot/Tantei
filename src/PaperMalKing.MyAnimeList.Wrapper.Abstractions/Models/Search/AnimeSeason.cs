// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Text.Json.Serialization;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Converters;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Search;

[JsonConverter(typeof(UnknownJsonStringEnumConverter<AnimeSeason>))]
public enum AnimeSeason : byte
{
	[JsonStringEnumMemberName("unknown")]
	Unknown = 0,

	[JsonStringEnumMemberName("winter")]
	Winter = 1,

	[JsonStringEnumMemberName("spring")]
	Spring = 2,

	[JsonStringEnumMemberName("summer")]
	Summer = 3,

	[JsonStringEnumMemberName("fall")]
	Fall = 4,
}
