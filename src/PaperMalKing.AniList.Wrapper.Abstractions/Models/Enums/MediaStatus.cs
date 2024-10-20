// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<MediaStatus>))]
public enum MediaStatus : byte
{
	[JsonStringEnumMemberName("FINISHED")]
	Finished = 0,

	[JsonStringEnumMemberName("RELEASING")]
	Releasing = 1,

	[JsonStringEnumMemberName("NOT_YET_RELEASED")]
	NotYetReleased = 2,

	[JsonStringEnumMemberName("CANCELLED")]
	Cancelled = 3,

	[JsonStringEnumMemberName("HIATUS")]
	Hiatus = 4,
}