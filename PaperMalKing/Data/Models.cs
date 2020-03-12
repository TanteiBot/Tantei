using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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