// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using Microsoft.Extensions.EnumStrings;
using PaperMalKing.Common.Attributes;

namespace PaperMalKing.AniList.UpdateProvider;

[EnumStrings]
public enum AniListUpdateType : byte
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
	[EnumDescription("paused manga", "updates for paused manga")]
	PausedManga = 10,
	[EnumDescription("rereading manga", "updates for manga being reread")]
	RereadingManga = 11,
	[EnumDescription("added favourite", "updates for favourite being added")]
	FavouriteAdded = 12,
	[EnumDescription("removed favourite", "updates for favourite being removed")]
	FavouriteRemoved = 13,
	[EnumDescription("created review", "updates for review being created")]
	ReviewCreated = 14,
}