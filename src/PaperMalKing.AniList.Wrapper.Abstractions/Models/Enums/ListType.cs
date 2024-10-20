// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.Text.Json.Serialization;
using Microsoft.Extensions.EnumStrings;

namespace PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<ListType>))]
[EnumStrings]
public enum ListType : byte
{
	[JsonStringEnumMemberName("ANIME")]
	Anime = 0,

	[JsonStringEnumMemberName("Manga")]
	Manga = 1,
}