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
		
		public List<MalFavoriteAnime> FavoriteAnimes { get; set; } = null!;

		public List<MalFavoriteManga> FavoriteMangas { get; set; } = null!;

		public List<MalFavoriteCharacter> FavoriteCharacters { get; set; } = null!;

		public List<MalFavoritePerson> FavoritePeople { get; set; } = null!;
	}
}