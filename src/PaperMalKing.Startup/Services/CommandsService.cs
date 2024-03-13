// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.EventArgs;
using Microsoft.Extensions.Logging;
using PaperMalKing.UpdatesProviders.Base;

namespace PaperMalKing.Startup.Services;

internal sealed class CommandsService : ICommandsService
{
	private readonly ILogger<CommandsService> _logger;

	public SlashCommandsExtension SlashCommandsExtension { get; }

	[SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates", Justification = "It's meant to be a singleton")]
	public CommandsService(IServiceProvider provider, DiscordClient client, ILogger<CommandsService> logger)
	{
		this._logger = logger;
		this._logger.LogTrace("Building Commands service");

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
			this._logger.LogTrace("Found {Assembly} which may contain Commands modules", assembly);
			foreach (var type in assembly.DefinedTypes.Where(t => t.FullName!.EndsWith("Commands", StringComparison.OrdinalIgnoreCase)))
			{
				this._logger.LogTrace("Trying to register {@Type} command module", type);
				try
				{
					if (nestedTypesNotToRegister.Contains(type))
					{
						continue;
					}

					var nestedTypes = type.GetNestedTypes(BindingFlags.Public)
										  .Where(t => t.FullName!.EndsWith("Commands", StringComparison.OrdinalIgnoreCase));
					foreach (var nestedType in nestedTypes)
					{
						nestedTypesNotToRegister.Add(nestedType);
					}

					this.SlashCommandsExtension.RegisterCommands(type);
				}
				#pragma warning disable CA1031
				// Modify '.ctor' to catch a more specific allowed exception type, or rethrow the exception
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
		this._logger.CommandSuccessfullyExecuted(e.Context.CommandName, e.Context.Member);
		return Task.CompletedTask;
	}

	private Task SlashCommandsExtensionOnSlashCommandErroredAsync(SlashCommandsExtension sender, SlashCommandErrorEventArgs e)
	{
		this._logger.CommandErrored(e.Exception, e.Context.CommandName, e.Context.Member);
		return Task.CompletedTask;
	}
}