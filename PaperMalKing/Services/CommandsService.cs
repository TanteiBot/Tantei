// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using Microsoft.Extensions.Logging;
using PaperMalKing.UpdatesProviders.Base;

namespace PaperMalKing.Services;

public sealed class CommandsService : ICommandsService
{
	private readonly ILogger<CommandsService> _logger;
	public SlashCommandsExtension SlashCommandsExtension { get; }

	public CommandsService(IServiceProvider provider, DiscordClient client, ILogger<CommandsService> logger)
	{
		this._logger = logger;
		this._logger.LogTrace("Building Commands service");

		this.SlashCommandsExtension = client.UseSlashCommands(new ()
		{
			Services = provider
		});
		this.SlashCommandsExtension.SlashCommandErrored += this.SlashCommandsExtensionOnSlashCommandErroredAsync;
		this.SlashCommandsExtension.SlashCommandExecuted += this.SlashCommandsExtensionOnSlashCommandExecutedAsync;

		var assemblies = AppDomain.CurrentDomain.GetAssemblies();
		HashSet<Type> nestedTypesNotToRegister = new();

		foreach (var assembly in assemblies.Where(a => a.FullName?.Contains("PaperMalKing", StringComparison.OrdinalIgnoreCase) ?? true))
		{
			nestedTypesNotToRegister.Clear();
			this._logger.LogTrace("Found {Assembly} which may contain Commands modules", assembly);
			foreach (var type in assembly.GetExportedTypes().Where(t => t.FullName!.EndsWith("Commands", StringComparison.OrdinalIgnoreCase)))
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

					this.SlashCommandsExtension.RegisterCommands(type);
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

		this._logger.LogTrace("Building Commands service finished");
	}

	private Task SlashCommandsExtensionOnSlashCommandExecutedAsync(SlashCommandsExtension sender, SlashCommandExecutedEventArgs e)
	{
		this._logger.LogDebug("{Command} was successfully executed by request of {Member}", e.Context.CommandName, e.Context.Member);
		return Task.CompletedTask;
	}

	private Task SlashCommandsExtensionOnSlashCommandErroredAsync(SlashCommandsExtension sender, SlashCommandErrorEventArgs e)
	{
		this._logger.LogError(e.Exception, "{Command} errored with exception while trying to be executed by {Member}", e.Context.CommandName,
			e.Context.Member);
		return Task.CompletedTask;
	}
}