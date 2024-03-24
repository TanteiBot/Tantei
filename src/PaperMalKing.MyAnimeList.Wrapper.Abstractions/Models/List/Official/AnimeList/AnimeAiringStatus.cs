// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.ComponentModel;
using System.Text.Json.Serialization;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.AnimeList;

[JsonConverter(typeof(JsonStringEnumConverter<AnimeAiringStatus>))]
public enum AnimeAiringStatus : byte
{
	uknown = 0,
	[Description("Finished airing")]
	finished_airing = 1,
	[Description("Currently airing")]
	currently_airing = 2,
	[Description("Not yet aired")]
	not_yet_aired = 3,
}