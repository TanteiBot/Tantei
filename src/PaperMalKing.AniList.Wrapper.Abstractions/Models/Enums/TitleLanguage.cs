// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<TitleLanguage>))]
public enum TitleLanguage : byte
{
	[JsonStringEnumMemberName("NATIVE")]
	Native = 1,

	[JsonStringEnumMemberName("NATIVE_STYLISED")]
	NativeStylised = 2,

	[JsonStringEnumMemberName("ROMAJI")]
	Romaji = 3,

	[JsonStringEnumMemberName("ROMAJI_STYLISED")]
	RomajiStylised = 4,

	[JsonStringEnumMemberName("ENGLISH")]
	English = 5,

	[JsonStringEnumMemberName("ENGLISH_STYLISED")]
	EnglishStylised = 6,
}