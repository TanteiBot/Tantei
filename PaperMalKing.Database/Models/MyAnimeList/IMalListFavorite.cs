// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
namespace PaperMalKing.Database.Models.MyAnimeList;

public interface IMalListFavorite : IMalFavorite
{
	public string Type { get; init; }

	public uint StartYear { get; init; }
}