// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Text.Json.Serialization;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions.Converters;

namespace PaperMalKing.MyAnimeList.Wrapper.Abstractions.Models.Search;

[JsonConverter(typeof(UnknownJsonStringEnumConverter<NsfwCategory>))]
public enum NsfwCategory : byte
{
	[JsonStringEnumMemberName("unknown")]
	Unknown = 0,

	[JsonStringEnumMemberName("white")]
	White = 1,

	[JsonStringEnumMemberName("gray")]
	Gray = 2,

	[JsonStringEnumMemberName("black")]
	Black = 3,
}
