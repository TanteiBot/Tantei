// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.Text.Json.Serialization;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.List.Official.MangaList;

[JsonConverter(typeof(JsonStringEnumConverter<MangaPublishingStatus>))]
public enum MangaPublishingStatus : byte
{
	[JsonStringEnumMemberName("unknown")]
	Unknown = 0,

	[JsonStringEnumMemberName("finished")]
	Finished = 1,

	[JsonStringEnumMemberName("currently_publishing")]
	CurrentlyPublishing = 2,

	[JsonStringEnumMemberName("not_yet_published")]
	NotYetPublished = 3,

	[JsonStringEnumMemberName("discontinued")]
	Discontinued = 4,

	[JsonStringEnumMemberName("on_hiatus")]
	OnHiatus = 5,
}