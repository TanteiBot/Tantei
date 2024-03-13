// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<TitleLanguage>))]
public enum TitleLanguage : byte
{
	NATIVE = 1,
	NATIVE_STYLISED = 2,
	ROMAJI = 3,
	ROMAJI_STYLISED = 4,
	ENGLISH = 5,
	ENGLISH_STYLISED = 6,
}