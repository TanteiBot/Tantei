// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System.Diagnostics.CodeAnalysis;

namespace PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;

[SuppressMessage("Naming", "CA1707")]
public enum MediaFormat : byte
{
	TV,
	TV_SHORT,
	MOVIE,
	SPECIAL,
	OVA,
	ONA,
	MUSIC,
	MANGA,
	NOVEL,
	ONE_SHOT
}