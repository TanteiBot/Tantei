// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
namespace PaperMalKing.Common.Enums;

public enum FavoriteType : byte
{
	Anime = 0,
	Manga = 1,
	Character = 2,
	Person = 3,
	BaseList = byte.MaxValue - 1,
	Base = byte.MaxValue
}