// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System.Collections.Generic;
using PaperMalKing.AniList.Wrapper.Models;
using PaperMalKing.AniList.Wrapper.Models.Responses;

#pragma warning disable CA1002
namespace PaperMalKing.AniList.UpdateProvider.CombinedResponses
{
    public sealed class CombinedFavouritesInfoResponse
    {
		public List<Media> Anime { get; } = new();
		public List<Media> Manga { get; } = new();
		public List<Character> Characters { get; } = new();
		public List<Staff> Staff { get; } = new();
		public List<Studio> Studios { get; } = new();

		public void Add(FavouritesResponse response)
        {
            this.Anime.AddRange(response.Anime.Values);
            this.Manga.AddRange(response.Manga.Values);
            this.Characters.AddRange(response.Characters.Values);
            this.Staff.AddRange(response.Staff.Values);
            this.Studios.AddRange(response.Studios.Values);
        }
    }
}