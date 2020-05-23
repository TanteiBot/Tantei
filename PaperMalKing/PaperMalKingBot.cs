using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using PaperMalKing.Commands;
using PaperMalKing.Data;
using PaperMalKing.MyAnimeList.FeedReader;
using PaperMalKing.Services;
using PaperMalKing.Utilities;

namespace PaperMalKing
{
	public sealed class PaperMalKingBot
	{
		private DiscordClient Client { get; }

		private CommandsNextExtension Commands { get; }

		private readonly BotConfig _config;

		private readonly string _logName;

		private readonly object _logLock = new object();

		private readonly ClockService _clock;

		private readonly DiscordActivity _activity;

		public PaperMalKingBot(BotConfig config)
		{
			this._config = config;

			var discordCfg = this._config.Discord;

			this._logName = discordCfg.LogName;

			var discordConfig = new DiscordConfiguration
			{
				LogLevel = LogLevel.Debug,
				UseInternalLogHandler = false,
				Token = discordCfg.Token,
				ReconnectIndefinitely = discordCfg.ReconnectIndefinitely,
				AutoReconnect = discordCfg.AutoReconnect,
				MessageCacheSize = discordCfg.MessageCacheSize
			};

			var actType = (ActivityType) this._config.Discord.ActivityType;
			this._activity = new DiscordActivity(this._config.Discord.PresenceText, actType);


			this.Client = new DiscordClient(discordConfig);
			this.Client.Ready += this.Client_Ready;
			this.Client.Ready += this.Client_PresenceReady;
			this.Client.ClientErrored += this.Client_ClientErrored;
			this.Client.DebugLogger.LogMessageReceived += this.DebugLogger_LogMessageReceived;
			this.Client.GuildDownloadCompleted += this.Client_GuildDownloadCompleted;
			this._clock = new ClockService();
			var malRssService = new FeedReader(this.Client.DebugLogger.LogMessage, this._clock,
				this._config.MyAnimeList);
			var services = new ServiceCollection()
						   .AddSingleton(this.Client)
						   .AddSingleton(this._config)
						   .AddSingleton(this._clock)
						   .AddSingleton(malRssService)
						   .AddSingleton(new MalService(this._config, this.Client, malRssService, this._clock))
						   .AddScoped<DatabaseContext>()
						   .BuildServiceProvider();


			var cmdCfg = this._config.Discord.Commands;

			var cmdConfig = new CommandsNextConfiguration
			{
				CaseSensitive = cmdCfg.CaseSensitive,
				DmHelp = cmdCfg.DmHelp,
				EnableDms = false,
				EnableMentionPrefix = cmdCfg.EnableMentionPrefix,
				StringPrefixes = cmdCfg.Prefixes,
				Services = services
			};

			this.Commands = this.Client.UseCommandsNext(cmdConfig);

			this.Commands.CommandErrored += this.Commands_CommandErrored;
			this.Commands.CommandExecuted += this.Commands_CommandExecuted;

			this.Commands.RegisterCommands<MalCommands>();
			this.Commands.RegisterCommands<UngruppedCommands>();

			this.Commands.SetHelpFormatter<PaperMalKingHelpFormatter>();
		}

		private Task Client_PresenceReady(ReadyEventArgs e)
		{
			return e.Client.UpdateStatusAsync(this._activity, UserStatus.Online);
		}

		public async Task Start()
		{
			this.Client.DebugLogger.LogMessage(LogLevel.Info, this._logName, "Starting bot", this._clock.Now);
			await this.Client.ConnectAsync(this._activity, UserStatus.Online);
			await Task.Delay(-1);
		}

		private Task Client_ClientErrored(ClientErrorEventArgs e)
		{
			var ex = e.Exception;
			while (ex is AggregateException || ex.InnerException != null)
				ex = ex.InnerException;

			e.Client.DebugLogger.LogMessage(LogLevel.Error, this._logName,
				$"Exception occured: {ex.GetType()}: {ex.Message}", this._clock.Now);
			return Task.CompletedTask;
		}

		private Task Client_Ready(ReadyEventArgs e)
		{
			Console.Title = this._logName;
			e.Client.DebugLogger.LogMessage(LogLevel.Info, this._logName, "Bot is ready", this._clock.Now);
			this.Client.Ready -= this.Client_Ready;
			return Task.CompletedTask;
		}

		private Task Client_GuildDownloadCompleted(GuildDownloadCompletedEventArgs e)
		{
			foreach (var guild in e.Guilds.Values)
				this.Client.DebugLogger.LogMessage(LogLevel.Info, this._logName, $"Guild available {guild.Name}",
					this._clock.Now);
			return Task.CompletedTask;
		}

		private Task Commands_CommandExecuted(CommandExecutionEventArgs e)
		{
			e.Context.Client.DebugLogger.LogMessage(LogLevel.Info, this._logName,
				$"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'", this._clock.Now);
			return Task.CompletedTask;
		}

		private async Task Commands_CommandErrored(CommandErrorEventArgs e)
		{
			e.Context.Client.DebugLogger.LogMessage(LogLevel.Error, this._logName,
				$"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}",
				this._clock.Now);
			var ex = e.Exception;
			while (ex is AggregateException || ex.InnerException != null)
			{
				ex = ex.InnerException;
			}

			if (ex is CommandNotFoundException commandNotFoundEx)
			{
				var errorEmbed = EmbedTemplate.ErrorEmbed(e.Context.User,
					$"Command with name '{commandNotFoundEx.CommandName}' not found. Try using '{this._config.Discord.Commands.Prefixes[0]}help' to get a list of available commands.");
				await e.Context.RespondAsync(embed: errorEmbed);
			}
			else if (ex is ChecksFailedException checksFailedEx)
			{
				var failedCheck = checksFailedEx.FailedChecks.First();

				if (failedCheck is RequirePermissionsAttribute reqPerm)
				{
					var errorEmbed = EmbedTemplate.CommandErrorEmbed(e.Command, e.Context.User,
						message: $"You are lacking permissions:{reqPerm.Permissions.ToPermissionString()}");
					await e.Context.RespondAsync(embed: errorEmbed);
				}
			}
			else
			{
				var errorEmbed = EmbedTemplate.CommandErrorEmbed(e.Command, e.Context.User, ex);
				await e.Context.RespondAsync(embed: errorEmbed);
			}
		}

		private void DebugLogger_LogMessageReceived(object sender, DebugLogMessageEventArgs e)
		{
			lock (this._logLock)
			{
				if (e.Level == LogLevel.Debug)
					Console.ForegroundColor = ConsoleColor.DarkGreen;
				else if (e.Level == LogLevel.Info)
					Console.ForegroundColor = ConsoleColor.White;
				else if (e.Level == LogLevel.Warning)
					Console.ForegroundColor = ConsoleColor.DarkYellow;
				else if (e.Level == LogLevel.Error)
					Console.ForegroundColor = ConsoleColor.DarkRed;
				else if (e.Level == LogLevel.Critical)
				{
					Console.BackgroundColor = ConsoleColor.DarkRed;
					Console.ForegroundColor = ConsoleColor.Black;
				}

				string timestampFormat;
#if DEBUG
				timestampFormat = "dd.MM.yy HH\\:mm\\:ss.fff";
#else
                timestampFormat = "dd.MM.yy HH\\:mm\\:ss";
#endif

				Console.Write(
					$"[{e.Timestamp.ToString(timestampFormat)}] [{e.Application.ToFixedWidth(14)}] [{e.Level.ToShortName()}]");
				Console.ResetColor();
				Console.WriteLine($" {e.Message}{(e.Exception != null ? $"\n{e.Exception}" : "")}");
			}
		}
	}
}