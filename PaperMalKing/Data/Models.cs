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

		public virtual List<GuildUsers> Guilds { get; set; }
	}

	[Table("Guilds")]
	public class PmkGuild
	{
		[Key]
		[Required]
		[Column("GuildId")]
		[DatabaseGenerated(DatabaseGeneratedOption.None)]
		public long GuildId { get; set; }

		[Column("ChannelId")]
		public long? ChannelId { get; set; }
		public virtual List<GuildUsers> Users { get; set; }
	}

	public class GuildUsers
	{
		[ForeignKey("DiscordId")]
		public long DiscordId { get; set; }

		public virtual PmkUser User { get; set; }

		[ForeignKey("GuildId")]
		public long GuildId { get; set; }

		public virtual PmkGuild Guild{ get; set; }
	}
}


