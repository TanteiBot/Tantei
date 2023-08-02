// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<FavouriteType>))]
public enum FavouriteType : byte
{
	Anime = 0,
	Manga = 1,
	Characters = 2,
	Staff = 3,
	Studios = 4,
}