using YamlDotNet.Serialization;

namespace PaperMalKing.Data
{
	/// <summary>
	/// Config for bot
	/// </summary>
	public sealed class BotConfig
	{
		[YamlMember(Alias = "Misc")]
		public BotMiscConfig Misc { get; private set; }

		/// <summary>
		/// Bot Discord config
		/// </summary>
		[YamlMember(Alias = "Discord")]
		public BotDiscordConfig Discord { get; private set; }

		/// <summary>
		/// Bot database config
		/// </summary>
		[YamlMember(Alias ="Database")]
		public BotDatabaseConfig Database { get; private set; }

		[YamlMember(Alias = "Jikan")]
		public BotJikanConfig Jikan { get; private set; }

		[YamlMember(Alias = "MyAnimeList")]
		public BotMalConfig MyAnimeList { get; private set; }
	}

	public class BotMiscConfig
	{
		[YamlMember(Alias = "Timeout")]
		public int Timeout { get; private set; }
	}


	/// <summary>
	/// Bot config options related to Discord
	/// </summary>
	public sealed class BotDiscordConfig
	{
		/// <summary>
		/// Bot Discord API Token
		/// </summary>
		[YamlMember(Alias ="Token")]
		public string Token { get; private set; }

		/// <summary>
		/// Should bot reconnect to Discord indefinitely
		/// </summary>
		[YamlMember(Alias = "ReconnectIndefinitely")]
		public bool ReconnectIndefinitely { get; private set; }

		/// <summary>
		/// Should bot autoreconnect to Discord
		/// </summary>
		[YamlMember(Alias = "AutoReconnect")]
		public bool AutoReconnect { get; private set; }

		/// <summary>
		/// Size of the global message cache.
		/// </summary>
		[YamlMember(Alias = "MessageCacheSize")]
		public int MessageCacheSize { get; private set; }

		/// <summary>
		/// Bot name that will be written to logs and console window title
		/// </summary>
		[YamlMember(Alias = "LogName")]
		public string LogName { get; private set; }

		/// <summary>
		/// Which activity type will appear in bot status.
		/// Set to 0 to display "playing" activity.
		/// Set to 2 to display "listening" activity.
		/// Set to 3 to display "watching" activity
		/// </summary>
		[YamlMember(Alias = "ActivityType")]
		public int ActivityType { get; private set; }

		/// <summary>
		/// Text that will be displayed in bot status
		/// </summary>
		[YamlMember(Alias = "PresenceText")]
		public string PresenceText { get; private set; }

		/// <summary>
		/// Bot config to commands in Discord
		/// </summary>
		[YamlMember(Alias = "Commands")]
		public BotDiscordCommandsConfig Commands { get; private set; }

		/// <summary>
		/// Bot Discord config options related to commands
		/// </summary>
		public sealed class BotDiscordCommandsConfig
		{
			/// <summary>
			/// Prefixes that appliable to commands
			/// </summary>
			[YamlMember(Alias = "Prefixes")]
			public string[] Prefixes { get; private set; }

			/// <summary>
			/// Should bot mention be considered as prefix
			/// </summary>
			[YamlMember(Alias = "EnableMentionPrefix")]
			public bool EnableMentionPrefix { get; private set; }

			/// <summary>
			/// Should bot commands names be case sensitive
			/// </summary>
			[YamlMember(Alias = "CaseSensitive")]
			public bool CaseSensitive { get; private set; }

			/// <summary>
			/// Should bot sends commands help to DM
			/// </summary>
			[YamlMember(Alias = "DmHelp")]
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
		[YamlMember(Alias = "ConnectionString")]
		public string ConnectionString { get; private set; }
	}


	public sealed class BotMalConfig : IRateLimitable
	{
		/// <inheritdoc />
		[YamlMember(Alias = "RateLimit")]
		public RateLimitConfig RateLimit { get; set; }

		[YamlMember(Alias = "DelayBetweenUpdateChecks")]
		public int DelayBetweenUpdateChecks { get; set; }
	}

	public sealed class BotJikanConfig : IRateLimitable
	{
		/// <inheritdoc />
		[YamlMember(Alias = "RateLimit")]
		public RateLimitConfig RateLimit { get; set; }

		[YamlMember(Alias = "Uri")]
		public string Uri { get; private set; }
	}

	public class RateLimitConfig
	{
		/// <summary>
		/// Amount of requests
		/// </summary>
		[YamlMember(Alias = "RequestsCount")]
		public int RequestsCount { get; private set; }

		/// <summary>
		/// Time in milliseconds after which amount of available requests will be reset
		/// </summary>
		[YamlMember(Alias = "TimeConstraint")]
		public double TimeConstraint { get; private set; }
	}

	public interface IRateLimitable
	{
		public RateLimitConfig RateLimit { get; set; }
	}
}