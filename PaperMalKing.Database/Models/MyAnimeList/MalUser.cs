// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaperMalKing.Database.Models.MyAnimeList
{
	public sealed class MalUser
	{
		public DiscordUser DiscordUser { get; init; } = null!;

		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		[Key]
		public int UserId { get; init; }

		public string Username { get; set; } = null!;

		public DateTimeOffset LastUpdatedAnimeListTimestamp { get; set; }

		public DateTimeOffset LastUpdatedMangaListTimestamp { get; set; }

		public string LastAnimeUpdateHash { get; set; } = null!;

		public string LastMangaUpdateHash { get; set; } = null!;
		
		public MalUserFeatures Features { get; set; }
		
		public List<MalFavoriteAnime> FavoriteAnimes { get; set; } = null!;

		public List<MalFavoriteManga> FavoriteMangas { get; set; } = null!;

		public List<MalFavoriteCharacter> FavoriteCharacters { get; set; } = null!;

		public List<MalFavoritePerson> FavoritePeople { get; set; } = null!;

		public List<MalFavoriteCompany> FavoriteCompanies { get; set; } = null!;
	}
}