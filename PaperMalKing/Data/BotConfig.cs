using System.Threading;
using Newtonsoft.Json;

namespace PaperMalKing.Data
{
	/// <summary>
	/// Config for bot
	/// </summary>
	public sealed class BotConfig
	{
		/// <summary>
		/// Bot Discord config
		/// </summary>
		[JsonProperty("Discord")]
		public BotDiscordConfig Discord { get; private set; }

		/// <summary>
		/// Bot database config
		/// </summary>
		[JsonProperty("Database")]
		public BotDatabaseConfig Database { get; private set; }

		[JsonProperty("Jikan")]
		public BotJikanConfig Jikan { get; private set; }

		[JsonProperty("MyAnimeList")]
		public BotMalConfig MyAnimeList { get; private set; }
	}


	/// <summary>
	/// Bot config options related to Discord
	/// </summary>
	public sealed class BotDiscordConfig
	{
		/// <summary>
		/// Bot Discord API Token
		/// </summary>
		[JsonProperty("Token")]
		public string Token { get; private set; }

		/// <summary>
		/// Should bot reconnect to Discord indefinitely
		/// </summary>
		[JsonProperty("ReconnectIndefinitely")]
		public bool ReconnectIndefinitely { get; private set; }

		/// <summary>
		/// Should bot autoreconnect to Discord
		/// </summary>
		[JsonProperty("AutoReconnect")]
		public bool AutoReconnect { get; private set; }

		/// <summary>
		/// Size of the global message cache. 
		/// </summary>
		[JsonProperty("MessageCacheSize")]
		public int MessageCacheSize { get; private set; }

		/// <summary>
		/// Bot name that will be written to logs and console window title
		/// </summary>
		[JsonProperty("LogName")]
		public string LogName { get; private set; }

		/// <summary>
		/// Which activity type will appear in bot status.
		/// Set to 0 to display "playing" activity.
		/// Set to 2 to display "listening" activity.
		/// Set to 3 to display "watching" activity
		/// </summary>
		[JsonProperty("ActivityType")]
		public int ActivityType { get; private set; }

		/// <summary>
		/// Text that will be displayed in bot status
		/// </summary>
		[JsonProperty("PresenceText")]
		public string PresenceText { get; private set; }

		/// <summary>
		/// Bot config to commands in Discord
		/// </summary>
		[JsonProperty("Commands")]
		public BotDiscordCommandsConfig Commands { get; private set; }

		/// <summary>
		/// Bot Discord config options related to commands
		/// </summary>
		public sealed class BotDiscordCommandsConfig
		{
			/// <summary>
			/// Prefixes that appliable to commands
			/// </summary>
			[JsonProperty("Prefixes")]
			public string[] Prefixes { get; private set; }

			/// <summary>
			/// Should bot mention be considered as prefix
			/// </summary>
			[JsonProperty("EnableMentionPrefix")]
			public bool EnableMentionPrefix { get; private set; }

			/// <summary>
			/// Should bot commands names be case sensitive
			/// </summary>
			[JsonProperty("CaseSensitive")]
			public bool CaseSensitive { get; private set; }

			/// <summary>
			/// Should bot sends commands help to DM
			/// </summary>
			[JsonProperty("DmHelp")]
			public bool DmHelp { get; private set; }
		}
	}

	/// <summary>
	/// Bot config options related to database
	/// </summary>
	public sealed class BotDatabaseConfig
	{
		/// <summary>
		/// Connection string to your SQLite database, generally it should look like "Data Source=Path_to_your_database_here"
		/// </summary>
		[JsonProperty("ConnectionString")]
		public string ConnectionString { get; private set; }
	}


	public sealed class BotMalConfig : ITimeoutable, IRateLimitable
	{
		/// <inheritdoc />
		public int Timeout { get; set; }

		/// <inheritdoc />
		[JsonProperty("RateLimit")]
		public RateLimitConfig RateLimit { get; set; }

		[JsonProperty("DelayBetweenUpdateChecks")]
		public int DelayBetweenUpdateChecks { get; set; }
	}

	public sealed class BotJikanConfig : ITimeoutable, IRateLimitable
	{
		/// <inheritdoc />
		public int Timeout { get; set; }

		/// <inheritdoc />
		[JsonProperty("RateLimit")]
		public RateLimitConfig RateLimit { get; set; }

		[JsonProperty("Uri")]
		public string Uri { get; private set; }
	}

	public class RateLimitConfig
	{
		public RateLimitConfig()
		{
			/*
			 * This default config seems good for both MAL and Jikan.
			 *
			 * 
			 * Since there are no public API for MyAnimeList as well as documentation
			 * Default rate-limit will be 1 request every 2 seconds as it was advised to me in Jikan Discord Guild
			 * Source: https://discordapp.com/channels/460491088004907029/461199124205797439/676861111441686530
			 */

			this.RequestsCount = 1;
			this.TimeConstraint = 2000;
		}

		/// <summary>
		/// Amount of requests
		/// </summary>
		[JsonProperty("RequestsCount")]
		public int RequestsCount { get; private set; }

		/// <summary>
		/// Time in milliseconds after which amount of available requests will be reset
		/// </summary>
		[JsonProperty("TimeConstraintInMilliseconds")]
		public double TimeConstraint { get; private set; }
	}

	public interface IRateLimitable
	{
		public RateLimitConfig RateLimit { get; set; }
	}

	public interface ITimeoutable
	{
		/// <summary>
		/// Time in miliseconds before bot stops awaiting answer from remote server and cancels current check for updates
		/// </summary>
		[JsonProperty("Timeout")]
		public int Timeout { get; set; }
	}
}