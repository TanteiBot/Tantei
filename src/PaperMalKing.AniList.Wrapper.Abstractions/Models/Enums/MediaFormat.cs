// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<MediaFormat>))]
public enum MediaFormat : byte
{
	TV = 0,
	TV_SHORT = 1,
	MOVIE = 2,
	SPECIAL = 3,
	OVA = 4,
	ONA = 5,
	MUSIC = 6,
	MANGA = 7,
	NOVEL = 8,
	ONE_SHOT = 9,
}