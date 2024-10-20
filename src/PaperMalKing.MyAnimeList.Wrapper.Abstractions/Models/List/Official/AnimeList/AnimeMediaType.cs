// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.AnimeList;

[JsonConverter(typeof(JsonStringEnumConverter<AnimeMediaType>))]
public enum AnimeMediaType : byte
{
	[JsonStringEnumMemberName("unknown")]
	Unknown = 0,

	[JsonStringEnumMemberName("tv")]
	TV = 1,

	[JsonStringEnumMemberName("ova")]
	OVA = 2,

	[JsonStringEnumMemberName("movie")]
	Movie = 3,

	[JsonStringEnumMemberName("special")]
	Special = 4,

	[JsonStringEnumMemberName("ona")]
	ONA = 5,

	[JsonStringEnumMemberName("music")]
	Music = 6,

	[Description("TV Special")]
	[JsonStringEnumMemberName("tv_special")]
	TvSpecial = 7,

	[JsonStringEnumMemberName("cm")]
	CM = 8,

	[JsonStringEnumMemberName("pv")]
	PV = 9,
}