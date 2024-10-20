// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.MangaList;

[JsonConverter(typeof(JsonStringEnumConverter<MangaMediaType>))]
public enum MangaMediaType : byte
{
	[JsonStringEnumMemberName("unknown")]
	Unknown = 0,

	[JsonStringEnumMemberName("manga")]
	Manga = 1,

	[JsonStringEnumMemberName("novel")]
	Novel = 2,

	[Description("One-shot")]
	[JsonStringEnumMemberName("one_shot")]
	OneShot = 3,

	[JsonStringEnumMemberName("doujinshi")]
	Doujinshi = 4,

	[JsonStringEnumMemberName("manhwa")]
	Manhwa = 5,

	[JsonStringEnumMemberName("manhua")]
	Manhua = 6,

	[Description("Eol")]
	[JsonStringEnumMemberName("oel")]
	Oel = 7,

	[JsonStringEnumMemberName("light_novel")]
	LightNovel = 8,
}