#region LICENSE

// PaperMalKing.
// Copyright (C) 2021 N0D4N
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaperMalKing.Options;
using PaperMalKing.UpdatesProviders.Base;
using PaperMalKing.Utilities;

namespace PaperMalKing.Services
{
	public sealed class CommandsService : ICommandsService
	{
		private readonly ILogger<CommandsService> _logger;
		private readonly CommandsOptions _options;
		public CommandsNextExtension CommandsExtension { get; }

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
			Dictionary<Guid, Type> nestedTypesNotToRegister = new ();

			foreach (var assembly in assemblies.Where(a => a.FullName?.Contains("PaperMalKing", StringComparison.InvariantCultureIgnoreCase) ?? true))
			{
				nestedTypesNotToRegister.Clear();
				this._logger.LogTrace("Found {Assembly} which may contain Commands modules", assembly);
				foreach (var type in assembly.GetExportedTypes()
											 .Where(t => t.FullName!.EndsWith("Commands", StringComparison.InvariantCultureIgnoreCase)))
				{
					this._logger.LogTrace("Trying to register {@Type} command module", type);
					try
					{
						if(nestedTypesNotToRegister.TryGetValue(type.GUID, out _))
							continue;
						var nestedTypes = type.GetNestedTypes(BindingFlags.Public)
											  .Where(t => t.FullName!.EndsWith("Commands", StringComparison.InvariantCultureIgnoreCase));
						if (nestedTypes.Any())
							foreach (var nestedType in nestedTypes)
								nestedTypesNotToRegister.Add(nestedType.GUID, nestedType);
						
						this.CommandsExtension.RegisterCommands(type);
					}
					catch (Exception ex)
					{
						this._logger.LogError(ex, "Error occured while trying to register {FullName}", type.FullName);
					}

					this._logger.LogDebug("Successfully registered {@Type}", type);
				}
			}

			if (!this.CommandsExtension.RegisteredCommands.Any())
				this._logger.LogCritical("No commands were registered");

			this._logger.LogTrace("Building Commands service");
		}

		private Task CommandsExtensionOnCommandExecuted(CommandsNextExtension sender, CommandExecutionEventArgs e)
		{
			this._logger.LogDebug("{Command} was successfully executed by request of {Member}", e.Command, e.Context.Member);
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
								  "{Command} errored with exception while trying to be executed by {Member}", e.Command, e.Context.Member);
			return Task.CompletedTask;
		}
	}
}