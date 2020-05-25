using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PaperMalKing.MyAnimeList.Jikan.Data.Models;

namespace PaperMalKing.Data
{
	[Table("Users")]
	public class PmkUser
	{
		/// <summary>
		/// User's username on MyAnimeList
		/// </summary>
		[Required]
		[Column("MalUsername")]
		public string MalUsername { get; set; }

		/// <summary>
		/// Users id in Discord
		/// </summary>
		[Key]
		[Required]
		[Column("DiscordId")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public long DiscordId { get; set; }

		/// <summary>
		/// Date and Time when user's list was checked last
		/// </summary>
		[Required]
		[Column("LastUpdate")]
		public DateTime LastUpdateDate { get; set; }

		/// <summary>
		/// User's Id on MyAnimeList
		/// </summary>
		[Column("MalId")]
		public long? MalId { get; set; }

		/// <summary>
		/// Url to user's avatar on MyAnimeList
		/// </summary>
		[NotMapped]
		public string MalAvatarUrl
		{
			get
			{
				if (this._malAvatarUrl == null && this.MalId.HasValue)
				{
					var ts = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
					this._malAvatarUrl = $"https://cdn.myanimelist.net/images/userimages/{this.MalId.Value}.jpg?t={ts}";
				}

				return this._malAvatarUrl;
			}
		}

		[NotMapped]
		private string _malAvatarUrl;

		/// <summary>
		/// Url to user profile on MAL
		/// </summary>
		[NotMapped]
		public string Url => $"https://myanimelist.net/profile/{this.MalUsername}";

		/// <summary>
		/// Url to user's anime RSS feed
		/// </summary>
		[NotMapped]
		public string AnimeRssFeed => $"https://myanimelist.net/rss.php?type=rw&u={this.MalUsername}";

		/// <summary>
		/// Url to user's manga RSS feed
		/// </summary>
		[NotMapped]
		public string MangaRssFeed => $"https://myanimelist.net/rss.php?type=rm&u={this.MalUsername}";

		/// <summary>
		/// User's recently updated manga
		/// </summary>
		[NotMapped]
		public ICollection<MangaListEntry> RecentlyUpdatedManga { get; set; }

		/// <summary>
		/// User's recently update anime
		/// </summary>
		[NotMapped]
		public ICollection<AnimeListEntry> RecentlyUpdatedAnime { get; set; }

		/// <summary>
		/// Guilds to which user's updates will be send
		/// </summary>
		public virtual List<GuildUsers> Guilds { get; set; }
	}

	[Table("Guilds")]
	public class PmkGuild
	{
		/// <summary>
		/// Id of discord guild
		/// </summary>
		[Key]
		[Required]
		[Column("GuildId")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public long GuildId { get; set; }

		/// <summary>
		/// Id of channel where updates for users in this guild will be sent
		/// </summary>
		[Column("ChannelId")]
		public long? ChannelId { get; set; }

		/// <summary>
		/// Users which updates will be sent in this guild
		/// </summary>
		public virtual List<GuildUsers> Users { get; set; }
	}

	public class GuildUsers
	{
		/// <summary>
		/// User's Discord id
		/// </summary>
		[ForeignKey("DiscordId")]
		public long DiscordId { get; set; }

		public virtual PmkUser User { get; set; }

		/// <summary>
		/// Guild's Discord id
		/// </summary>
		[ForeignKey("GuildId")]
		public long GuildId { get; set; }

		public virtual PmkGuild Guild { get; set; }
	}
}