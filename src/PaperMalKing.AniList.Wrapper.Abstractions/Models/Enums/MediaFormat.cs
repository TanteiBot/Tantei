// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;

[SuppressMessage("Roslynator", "RCS1161:Enum should declare explicit values.")]
[JsonConverter(typeof(JsonStringEnumConverter<MediaFormat>))]
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