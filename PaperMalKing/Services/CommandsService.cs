// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

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

namespace PaperMalKing.Services;

public sealed class CommandsService : ICommandsService
{
	private readonly ILogger<CommandsService> _logger;
	public CommandsNextExtension CommandsExtension { get; }

	public CommandsService(IOptions<CommandsOptions> options, IServiceProvider provider, DiscordClient client, ILogger<CommandsService> logger)
	{
		this._logger = logger;
		this._logger.LogTrace("Building Commands service");

		var config = new CommandsNextConfiguration
		{
			CaseSensitive = options.Value.CaseSensitive,
			EnableMentionPrefix = options.Value.EnableMentionPrefix,
			StringPrefixes = options.Value.Prefixes,
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
		this.CommandsExtension.CommandErrored += this.CommandsExtensionOnCommandErroredAsync;
		this.CommandsExtension.CommandExecuted += this.CommandsExtensionOnCommandExecutedAsync;

		var assemblies = AppDomain.CurrentDomain.GetAssemblies();
		HashSet<Type> nestedTypesNotToRegister = new();

		foreach (var assembly in assemblies.Where(a => a.FullName?.Contains("PaperMalKing", StringComparison.OrdinalIgnoreCase) ?? true))
		{
			nestedTypesNotToRegister.Clear();
			this._logger.LogTrace("Found {Assembly} which may contain Commands modules", assembly);
			foreach (var type in assembly.GetExportedTypes()
										 .Where(t => t.FullName!.EndsWith("Commands", StringComparison.OrdinalIgnoreCase)))
			{
				this._logger.LogTrace("Trying to register {@Type} command module", type);
				try
				{
					if (nestedTypesNotToRegister.Contains(type))
						continue;
					var nestedTypes = type.GetNestedTypes(BindingFlags.Public)
										  .Where(t => t.FullName!.EndsWith("Commands", StringComparison.OrdinalIgnoreCase));
					foreach (var nestedType in nestedTypes)
						nestedTypesNotToRegister.Add(nestedType);

					this.CommandsExtension.RegisterCommands(type);
				}
#pragma warning disable CA1031
				catch (Exception ex)
#pragma warning restore CA1031
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

	private Task CommandsExtensionOnCommandExecutedAsync(CommandsNextExtension sender, CommandExecutionEventArgs e)
	{
		this._logger.LogDebug("{Command} was successfully executed by request of {Member}", e.Command, e.Context.Member);
		return Task.CompletedTask;
	}

	private Task CommandsExtensionOnCommandErroredAsync(CommandsNextExtension sender, CommandErrorEventArgs e)
	{
		if (e.Exception is CommandNotFoundException ex)
		{
			this._logger.LogDebug(ex, "Command wasn't found");
			return Task.CompletedTask;
		}

		this._logger.LogError(e.Exception, "{Command} errored with exception while trying to be executed by {Member}", e.Command,
			e.Context.Member);
		return Task.CompletedTask;
	}
}