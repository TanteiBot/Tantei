using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PaperMalKing.Database.Models.MyAnimeList
{
	[Table("MyAnimeListUsers")]
	public class User
	{
		public long DiscordUserId { get; set; }

		[Key]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public long UserId { get; private set; }

		public string Username { get; set; }

		public long LastUpdatedTimestamp { get; set; }

		public UserAnimeListColors AnimeListColors { get; set; } = null!;

		public UserMangaListColors MangaListColors { get; set; } = null!;

		public long FeaturesEnabled { get; set; }

		public List<UserFavoriteAnime> FavoriteAnimes { get; set; } = null!;

		public List<UserFavoriteManga> FavoriteMangas { get; set; } = null!;

		public List<UserFavoriteCharacter> FavoriteCharacters { get; set; } = null!;

		public List<UserFavoritePerson> FavoritePersons { get; set; } = null!;

		public User(long userId, string username, long lastUpdatedTimestamp, long featuresEnabled)
		{
			this.UserId = userId;
			this.Username = username;
			this.LastUpdatedTimestamp = lastUpdatedTimestamp;
			this.FeaturesEnabled = featuresEnabled;
		}
	}
}