// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using Microsoft.Extensions.Logging;
using PaperMalKing.Common;
using PaperMalKing.UpdatesProviders.Base;

namespace PaperMalKing.Startup.Services;

internal sealed class CommandsService : ICommandsService
{
	private readonly ILogger<CommandsService> _logger;

	public SlashCommandsExtension SlashCommandsExtension { get; }

	public CommandsService(IServiceProvider provider, DiscordClient client, ILogger<CommandsService> logger)
	{
		this._logger = logger;

		this.SlashCommandsExtension = client.UseSlashCommands(new()
		{
			Services = provider,
		});
		this.SlashCommandsExtension.SlashCommandErrored += this.SlashCommandsExtensionOnSlashCommandErroredAsync;
		this.SlashCommandsExtension.SlashCommandExecuted += this.SlashCommandsExtensionOnSlashCommandExecutedAsync;

		var assemblies = AppDomain.CurrentDomain.GetAssemblies();
		var nestedTypesNotToRegister = new HashSet<Type>();

		foreach (var assembly in assemblies.Where(a => a.FullName?.Contains("PaperMalKing", StringComparison.OrdinalIgnoreCase) ?? true))
		{
			nestedTypesNotToRegister.Clear();
			this._logger.FoundAssemblyWhichMayContainCommands(assembly);
			foreach (var type in assembly.DefinedTypes.Where(t => t.FullName!.EndsWith("Commands", StringComparison.OrdinalIgnoreCase) && !nestedTypesNotToRegister.Contains(t)))
			{
				this._logger.TryingToRegisterTypeAsCommandModule(type);
				try
				{
					var nestedTypes = type.GetNestedTypes(BindingFlags.Public)
										  .Where(t => t.FullName!.EndsWith("Commands", StringComparison.OrdinalIgnoreCase));
					nestedTypesNotToRegister.AddRange(nestedTypes);

					this.SlashCommandsExtension.RegisterCommands(type);
				}
#pragma warning disable CA1031
				// Modify '.ctor' to catch a more specific allowed exception type, or rethrow the exception
				catch (Exception ex)
#pragma warning restore CA1031
				{
					this._logger.ErrorOccuredWhileTryingToRegisterCommandModule(ex, type);
				}

				this._logger.SuccessfullyRegisteredType(type);
			}
		}

		this._logger.BuildingCommandsServiceFinished();
	}

	private Task SlashCommandsExtensionOnSlashCommandExecutedAsync(SlashCommandsExtension sender, SlashCommandExecutedEventArgs e)
	{
		this._logger.CommandSuccessfullyExecuted(e.Context.CommandName, e.Context.Member);
		return Task.CompletedTask;
	}

	private Task SlashCommandsExtensionOnSlashCommandErroredAsync(SlashCommandsExtension sender, SlashCommandErrorEventArgs e)
	{
		this._logger.CommandErrored(e.Exception, e.Context.CommandName, e.Context.Member);
		return Task.CompletedTask;
	}
}