// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System.Diagnostics.CodeAnalysis;

namespace PaperMalKing.AniList.Wrapper.Models.Enums
{
	[SuppressMessage("Microsoft.Design", "CA1008")]
	[SuppressMessage("Naming", "CA1707")]
	public enum TitleLanguage : byte
    {
		NATIVE = 1,
        NATIVE_STYLISED = 2,
        ROMAJI = 3,
        ROMAJI_STYLISED = 4,
        ENGLISH = 5,
        ENGLISH_STYLISED = 6
    }
}