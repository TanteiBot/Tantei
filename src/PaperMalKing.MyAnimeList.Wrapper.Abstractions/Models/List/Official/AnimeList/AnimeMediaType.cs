// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.AnimeList;

[JsonConverter(typeof(JsonStringEnumConverter<AnimeMediaType>))]
public enum AnimeMediaType : byte
{
	unknown = 0,
	[Description("TV")]
	tv = 1,
	[Description("OVA")]
	ova = 2,
	[Description("Movie")]
	movie = 3,
	[Description("Special")]
	special = 4,
	[Description("ONA")]
	ona = 5,
	[Description("Music")]
	music = 6,
	[Description("TV Special")]
	tv_special = 7,
	[Description("CM")]
	cm = 8,
	[Description("PV")]
	pv = 9,
}