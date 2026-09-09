// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<CharacterRole>))]
public enum CharacterRole : byte
{
	[JsonStringEnumMemberName("MAIN")]
	Main = 0,

	[JsonStringEnumMemberName("SUPPORTING")]
	Supporting = 1,

	[JsonStringEnumMemberName("BACKGROUND")]
	Background = 2,
}
