﻿// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.Text.Json.Serialization;

namespace PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;

[JsonConverter(typeof(JsonStringEnumConverter<ScoreFormat>))]
public enum ScoreFormat : byte
{
	POINT_100 = 0,
	POINT_10_DECIMAL = 1,
	POINT_10 = 2,
	POINT_5 = 3,
	POINT_3 = 4,
}