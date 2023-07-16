// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N
namespace PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;

public enum MediaListStatus : byte
{
	CURRENT = 0,
	PLANNING = 1,
	COMPLETED = 2,
	DROPPED = 3,
	PAUSED = 4,
	REPEATING = 5
}