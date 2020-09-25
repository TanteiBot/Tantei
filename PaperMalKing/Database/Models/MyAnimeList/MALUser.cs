using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaperMalKing.Database.Models.MyAnimeList
{
	[Table("MyAnimeListUsers")]
	public class MALUser
	{
		private DateTime _lastUpdatedAnimeListTimestamp;
		private DateTime _lastUpdatedMangaListTimestamp;
		public long DiscordUserId { get; set; }

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public long UserId { get; private set; }

		public string Username { get; set; }

		public DateTime LastUpdatedAnimeListTimestamp
		{
			get => this._lastUpdatedAnimeListTimestamp;
			set => this._lastUpdatedAnimeListTimestamp = value.ToUniversalTime();
		}

		public DateTime LastUpdatedMangaListTimestamp
		{
			get => this._lastUpdatedMangaListTimestamp;
			set => this._lastUpdatedMangaListTimestamp = value.ToUniversalTime();
		}

		public MALUserAnimeListColors AnimeListColors { get; set; } = null!;

		public MALUserMangaListColors MangaListColors { get; set; } = null!;

		public long FeaturesEnabled { get; set; }

		public List<MALUserFavoriteAnime> FavoriteAnimes { get; set; } = null!;

		public List<MALUserFavoriteManga> FavoriteMangas { get; set; } = null!;

		public List<MALUserFavoriteCharacter> FavoriteCharacters { get; set; } = null!;

		public List<MALUserFavoritePerson> FavoritePersons { get; set; } = null!;

		public MALUser(long userId, string username, long featuresEnabled,
					DateTime lastUpdatedAnimeListTimestamp, DateTime lastUpdatedMangaListTimestamp)
		{
			this.UserId = userId;
			this.Username = username;
			this.FeaturesEnabled = featuresEnabled;
			this.LastUpdatedAnimeListTimestamp = lastUpdatedAnimeListTimestamp;
			this.LastUpdatedMangaListTimestamp = lastUpdatedMangaListTimestamp;
		}
	}
}