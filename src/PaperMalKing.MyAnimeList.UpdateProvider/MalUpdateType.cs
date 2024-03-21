// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using PaperMalKing.Common.Attributes;

namespace PaperMalKing.MyAnimeList.UpdateProvider;

public enum MalUpdateType : byte
{
	[EnumDescription("watching", "updates for anime being watched")]
	Watching = 0,
	[EnumDescription("plan to watch", "updates for anime being planned to watch")]
	PlanToWatch = 1,
	[EnumDescription("completed anime", "updates for completed anime")]
	CompletedAnime = 2,
	[EnumDescription("dropped anime", "updates for dropped anime")]
	DroppedAnime = 3,
	[EnumDescription("on-hold anime", "updates for anime put on-hold")]
	OnHoldAnime = 4,
	[EnumDescription("rewatching anime", "updates for anime being rewatched")]
	RewatchingAnime = 5,
	[EnumDescription("reading", "updates for manga being read")]
	Reading = 6,
	[EnumDescription("plan to read", "updates for manga being planned to read")]
	PlanToRead = 7,
	[EnumDescription("completed manga", "updates for completed manga")]
	CompletedManga = 8,
	[EnumDescription("dropped manga", "updates for dropped manga")]
	DroppedManga = 9,
	[EnumDescription("on-hold manga", "updates for manga being put on-hold")]
	OnHoldManga = 10,
	[EnumDescription("rereading manga", "updates for manga being reread")]
	RereadingManga = 11,
	[EnumDescription("added favorite", "updates for favorite being added")]
	FavoriteAdded = 12,
	[EnumDescription("removed favorite", "updates for favorite being removed")]
	FavoriteRemoved = 13,
}