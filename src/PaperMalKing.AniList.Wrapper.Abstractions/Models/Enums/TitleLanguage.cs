// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
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

[SuppressMessage("Design", "MA0048:File name must match type name", Justification = "Extensions on enums may live in same file as enum")]
public static class TitleLanguageExtensions
{
	extension(TitleLanguage)
	{
		public static TitleLanguage Default => TitleLanguage.Romaji;
	}
}