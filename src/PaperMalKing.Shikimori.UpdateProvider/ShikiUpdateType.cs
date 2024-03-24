// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using PaperMalKing.Common.Attributes;

namespace PaperMalKing.Shikimori.UpdateProvider;

public enum ShikiUpdateType : byte
{
	[EnumDescription("watching", "updates for anime being watched")]
	Watching = 0,
	[EnumDescription("plan to watch", "updates for anime being planned to watch")]
	PlanToWatch = 1,
	[EnumDescription("completed anime", "updates for completed anime")]
	CompletedAnime = 2,
	[EnumDescription("dropped anime", "updates for dropped anime")]
	DroppedAnime = 3,
	[EnumDescription("paused anime", "updates for paused anime")]
	PausedAnime = 4,
	[EnumDescription("reading", "updates for manga being read")]
	Reading = 5,
	[EnumDescription("plan to read", "updates for manga being planned to read")]
	PlanToRead = 6,
	[EnumDescription("completed manga", "updates for completed manga")]
	CompletedManga = 7,
	[EnumDescription("dropped manga", "updates for dropped manga")]
	DroppedManga = 8,
	[EnumDescription("paused manga", "updates for paused manga")]
	PausedManga = 9,
	[EnumDescription("added favorite", "updates for favorite being added")]
	FavoriteAdded = 10,
	[EnumDescription("removed favorite", "updates for favorite being removed")]
	FavoriteRemoved = 11,
}