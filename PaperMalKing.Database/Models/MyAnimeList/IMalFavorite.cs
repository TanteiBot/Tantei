// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

namespace PaperMalKing.Database.Models.MyAnimeList;

public interface IMalFavorite
{
	public uint UserId { get; init; }

	public uint Id { get; init; }

	public string? ImageUrl { get; init; }

	public string Name { get; init; }

	public string NameUrl { get; init; }

	public MalUser User { get; init; }
}