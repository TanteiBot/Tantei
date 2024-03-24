// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using Microsoft.Extensions.EnumStrings;

namespace PaperMalKing.Shikimori.Wrapper.Abstractions.Models;

[EnumStrings(ExtensionClassModifiers = "public static")]
public enum HistoryRequestOptions : byte
{
	Anime = 0,
	Manga = 1,
	Any = 2,
}