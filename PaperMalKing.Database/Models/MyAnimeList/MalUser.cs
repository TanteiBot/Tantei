// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaperMalKing.Database.Models.MyAnimeList;

public sealed class MalUser
{
	public required DiscordUser DiscordUser { get; init; }

	[DatabaseGenerated(DatabaseGeneratedOption.None)]
	[Key]
	public int UserId { get; init; }

	public required string Username { get; set; }

	public DateTimeOffset LastUpdatedAnimeListTimestamp { get; set; }

	public DateTimeOffset LastUpdatedMangaListTimestamp { get; set; }

	public required string LastAnimeUpdateHash { get; set; }

	public required string LastMangaUpdateHash { get; set; }

	public MalUserFeatures Features { get; set; }

	public required List<MalFavoriteAnime> FavoriteAnimes { get; set; }

	public required List<MalFavoriteManga> FavoriteMangas { get; set; }

	public required List<MalFavoriteCharacter> FavoriteCharacters { get; set; }

	public required List<MalFavoritePerson> FavoritePeople { get; set; }

	public required List<MalFavoriteCompany> FavoriteCompanies { get; set; }
}