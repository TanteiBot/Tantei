using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaperMalKing.Options;
using PaperMalKing.Utilities;

namespace PaperMalKing.Services
{
	public sealed class CommandsService
	{
		private readonly ILogger<CommandsService> _logger;
		private readonly CommandsOptions _options;
		public readonly CommandsNextExtension CommandsExtension;

		public CommandsService(IOptions<CommandsOptions> options, IServiceProvider provider, DiscordClient client, ILogger<CommandsService> logger)
		{
			this._logger = logger;
			this._logger.LogTrace("Building Commands service");
			this._options = options.Value;

			var config = new CommandsNextConfiguration
			{
				CaseSensitive = this._options.CaseSensitive,
				EnableMentionPrefix = this._options.EnableMentionPrefix,
				StringPrefixes = this._options.Prefixes,
				DmHelp = false,
				Services = provider
			};

			this.CommandsExtension = client.UseCommandsNext(config);
			this.CommandsExtension.SetHelpFormatter<HelpFormatter>();
			this.CommandsExtension.RegisterUserFriendlyTypeName<string>("text");
			this.CommandsExtension.RegisterUserFriendlyTypeName<float>("floating point number");
			this.CommandsExtension.RegisterUserFriendlyTypeName<double>("floating point number");
			this.CommandsExtension.RegisterUserFriendlyTypeName<sbyte>("small integer");
			this.CommandsExtension.RegisterUserFriendlyTypeName<short>("small integer");
			this.CommandsExtension.RegisterUserFriendlyTypeName<int>("integer");
			this.CommandsExtension.RegisterUserFriendlyTypeName<long>("integer");
			this.CommandsExtension.RegisterUserFriendlyTypeName<byte>("unsigned small integer");
			this.CommandsExtension.RegisterUserFriendlyTypeName<ushort>("unsigned small integer");
			this.CommandsExtension.RegisterUserFriendlyTypeName<uint>("unsigned integer");
			this.CommandsExtension.RegisterUserFriendlyTypeName<ulong>("unsigned integer");
			this.CommandsExtension.CommandErrored += this.CommandsExtensionOnCommandErrored;
			this.CommandsExtension.CommandExecuted += this.CommandsExtensionOnCommandExecuted;

			var assemblies = Utils.LoadAndListPmkAssemblies();
			foreach (var assembly in assemblies.Where(a => a.FullName?.Contains("PaperMalKing", StringComparison.InvariantCultureIgnoreCase) ?? true))
			{
				this._logger.LogTrace($"Found {assembly.GetName().Name!} which may contain Commands modules");
				foreach (var type in assembly.GetExportedTypes()
											 .Where(t => t.FullName!.EndsWith("Commands", StringComparison.InvariantCultureIgnoreCase)))
				{
					this._logger.LogTrace($"Trying to register {type.FullName} command module");
					try
					{
						this.CommandsExtension.RegisterCommands(type);
					}
					catch (Exception ex)
					{
						this._logger.LogError(ex, $"Error occured while trying to register {type.FullName}");
					}

					this._logger.LogDebug($"Successfully registered {type}");
				}
			}

			if (!this.CommandsExtension.RegisteredCommands.Any())
				this._logger.LogCritical("No commands were registered");

			this._logger.LogTrace("Building Commands service");
		}

		private Task CommandsExtensionOnCommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
		{
			this._logger.LogDebug($"Command {e.Command.QualifiedName} was successfully executed by request of {e.Context.Member}");
			return Task.CompletedTask;
		}

		private Task CommandsExtensionOnCommandErrored(CommandsNextExtension sender, CommandErrorEventArgs e)
		{
			if (e.Exception is CommandNotFoundException ex)
			{
				this._logger.LogDebug(ex, "Command wasn't found");
				return Task.CompletedTask;
			}
			this._logger.LogError(e.Exception,
				$"Command {e.Command?.QualifiedName} errored with exception while trying to be executed by {e.Context.Member}");
			return Task.CompletedTask;
		}
	}
}