// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N
using System.Collections.Generic;
using PaperMalKing.AniList.Wrapper.Abstractions.Models;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Responses;

namespace PaperMalKing.AniList.UpdateProvider.CombinedResponses;

internal sealed class CombinedFavouritesInfoResponse
{
	public List<Media> Anime { get; } = [];

	public List<Media> Manga { get; } = [];

	public List<Character> Characters { get; } = [];

	public List<Staff> Staff { get; } = [];

	public List<Studio> Studios { get; } = [];

	public void Add(FavouritesResponse response)
	{
		this.Anime.AddRange(response.Anime.Values);
		this.Manga.AddRange(response.Manga.Values);
		this.Characters.AddRange(response.Characters.Values);
		this.Staff.AddRange(response.Staff.Values);
		this.Studios.AddRange(response.Studios.Values);
	}
}