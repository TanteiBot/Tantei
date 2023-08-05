// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<MediaStatus>))]
public enum MediaStatus : byte
{
	FINISHED = 0,
	RELEASING = 1,
	NOT_YET_RELEASED = 2,
	CANCELLED = 3,
	HIATUS = 4,
}