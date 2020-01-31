using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using PaperMalKing.Commands;
using PaperMalKing.Data;
using PaperMalKing.Services;
using PaperMalKing.Utilities;
using Microsoft.Extensions.DependencyInjection;

namespace PaperMalKing
{
	public sealed class PaperMalKingBot
	{
		private DiscordClient Client { get; }

		private CommandsNextExtension Commands { get; }

		private readonly BotConfig _config;

		private readonly string _logName;

		private readonly object _logLock = new object();

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
				ReconnectIndefinitely = discordCfg.ReconnectIndefinetely,
				AutoReconnect = discordCfg.AutoReconnect,
				MessageCacheSize = discordCfg.MessageCacheSize,
			};

			this.Client = new DiscordClient(discordConfig);
			this.Client.Ready += this.Client_Ready;
			this.Client.ClientErrored += this.Client_ClientErrored;
			this.Client.DebugLogger.LogMessageReceived += this.DebugLogger_LogMessageReceived;
			this.Client.GuildDownloadCompleted += this.Client_GuildDownloadCompleted;

			var services = new ServiceCollection()
			.AddSingleton(this.Client)
			.AddSingleton(this._config)
			.AddSingleton(new MalService(this._config,this.Client))
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
			this.Commands.RegisterCommands<OwnerCommands>();
			this.Commands.RegisterCommands<UngruppedCommands>();

			this.Commands.SetHelpFormatter<PaperMalKingHelpFormatter>();
		}

		public async Task Start()
		{
			this.Client.DebugLogger.LogMessage(LogLevel.Info, this._logName, "Starting bot", DateTime.Now);
			await this.Client.ConnectAsync();
			await Task.Delay(-1);
		}

		private Task Client_ClientErrored(ClientErrorEventArgs e)
		{
			var ex = e.Exception;
			while(ex is AggregateException || ex.InnerException != null)
				ex = ex.InnerException;

			e.Client.DebugLogger.LogMessage(LogLevel.Error, this._logName, $"Exception occured: {ex.GetType()}: {ex.Message}", DateTime.Now);
			return Task.CompletedTask;
		}

		private async Task Client_Ready(ReadyEventArgs e)
		{
			Console.Title = this._logName;
			e.Client.DebugLogger.LogMessage(LogLevel.Info, this._logName,"Bot is ready", DateTime.Now);
			var actType = (ActivityType) this._config.Discord.ActivityType;
			await e.Client.UpdateStatusAsync(new DiscordActivity(this._config.Discord.PresenceText, actType),
				UserStatus.Online);
			this.Client.Ready -= this.Client_Ready;
		}

		private Task Client_GuildDownloadCompleted(GuildDownloadCompletedEventArgs e)
		{
			foreach (var guild in e.Guilds.Values)
				this.Client.DebugLogger.LogMessage(LogLevel.Info, this._logName, $"Guild available {guild.Name}",DateTime.Now);
			return Task.CompletedTask;
		}

		private Task Commands_CommandExecuted(CommandExecutionEventArgs e)
		{
			e.Context.Client.DebugLogger.LogMessage(LogLevel.Info, this._logName,
				$"{e.Context.User.Username} successfully executed '{e.Command.QualifiedName}'", DateTime.Now);
			return Task.CompletedTask;
		}

		private async Task Commands_CommandErrored(CommandErrorEventArgs e)
		{
			e.Context.Client.DebugLogger.LogMessage(LogLevel.Error, this._logName,
				$"{e.Context.User.Username} tried executing '{e.Command?.QualifiedName ?? "<unknown command>"}' but it errored: {e.Exception.GetType()}: {e.Exception.Message ?? "<no message>"}",
				DateTime.Now);
			var ex = e.Exception;
			while (ex is AggregateException || ex.InnerException != null)
			{
				ex = ex.InnerException;
			}

			if(ex is CommandNotFoundException commandNotFoundEx)
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
						message: $"You are lacking permissons:{reqPerm.Permissions.ToPermissionString()}");
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

				Console.Write($"[{e.Timestamp:G}] [{e.Application}] [{e.Level.ToString()}]");
				Console.ResetColor();
				Console.WriteLine($" {e.Message}{(e.Exception != null ? $"\n{e.Exception}" : "")}");
			}
		}
	}
}
