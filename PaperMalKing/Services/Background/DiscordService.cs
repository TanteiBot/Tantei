using System;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaperMalKing.Options;

namespace PaperMalKing.Services.Background
{
	public sealed class DiscordService : IHostedService, IDisposable
	{
		private readonly IOptionsMonitor<DiscordOptions> _options;
		private readonly ILogger<DiscordService> _logger;
		public readonly DiscordClient Client;

		public DiscordService(ILoggerFactory loggerFactory, IOptionsMonitor<DiscordOptions> options)
		{
			this._options = options;
			this._logger = loggerFactory.CreateLogger<DiscordService>();
			var config = new DiscordConfiguration
			{
				Token = options.CurrentValue.Token,
				AutoReconnect = true,
				Intents = DiscordIntents.Guilds | DiscordIntents.GuildMembers | DiscordIntents.GuildMessages,
				LoggerFactory = loggerFactory
			};

			this.Client = new DiscordClient(config);
			options.OnChange(x =>
			{
				this.Client.UpdateStatusAsync(this.CreateActivityFromConfig()).Start();
			});
			this._logger.LogDebug($"Built {nameof(DiscordService)}");
		}

		/// <inheritdoc />
		public Task StartAsync(CancellationToken cancellationToken)
		{
			this._logger.LogDebug($"Starting {nameof(DiscordService)}");
			return this.Client.ConnectAsync(this.CreateActivityFromConfig(),
				UserStatus.Online);
		}

		/// <inheritdoc />
		public Task StopAsync(CancellationToken cancellationToken)
		{
			this._logger.LogDebug($"Stopping {nameof(DiscordService)}");
			return this.Client.DisconnectAsync();
		}

		/// <inheritdoc />
		public void Dispose()
		{
			this._logger.LogDebug($"Disposing {nameof(DiscordService)}");
			this.Client.Dispose();
		}
		
		private DiscordActivity CreateActivityFromConfig()
		{
			var activityType = ActivityType.Watching;
			if (Enum.TryParse<ActivityType>(this._options.CurrentValue.ActivityType, ignoreCase: true, out var result))
				activityType = result;

			return new DiscordActivity(this._options.CurrentValue.PresenceText, activityType);
		}
	}
}