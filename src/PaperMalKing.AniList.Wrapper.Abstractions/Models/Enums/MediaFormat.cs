// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<MediaFormat>))]
public enum MediaFormat : byte
{
	[JsonStringEnumMemberName("TV")]
	TV = 0,

	[Description("TV Short")]
	[JsonStringEnumMemberName("TV_SHORT")]
	TvShort = 1,

	[JsonStringEnumMemberName("MOVIE")]
	Movie = 2,

	[JsonStringEnumMemberName("SPECIAL")]
	Special = 3,

	[JsonStringEnumMemberName("OVA")]
	OVA = 4,

	[JsonStringEnumMemberName("ONA")]
	ONA = 5,

	[JsonStringEnumMemberName("MUSIC")]
	Music = 6,

	[JsonStringEnumMemberName("MANGA")]
	Manga = 7,

	[JsonStringEnumMemberName("NOVEL")]
	Novel = 8,

	[JsonStringEnumMemberName("ONE_SHOT")]
	OneShot = 9,
}