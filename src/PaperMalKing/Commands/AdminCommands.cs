// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Microsoft.Extensions.Hosting;
using PaperMalKing.Common;
using PaperMalKing.Services;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;

namespace PaperMalKing.Commands;

[SlashCommandGroup("admin", "Commands for owner")]
[SlashRequireOwner]
[SlashModuleLifespan(SlashModuleLifespan.Singleton)]
[SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods")]
internal sealed class
	AdminCommands : ApplicationCommandModule // We dont use bot commands module since most commands are immediately executed or dont provide any feedback
{
	private readonly UpdateProvidersConfigurationService _providersConfigurationService;
	private readonly UserCleanupService _cleanupService;
	private readonly IHostApplicationLifetime _lifetime;

	public AdminCommands(IHostApplicationLifetime lifetime, UpdateProvidersConfigurationService providersConfigurationService,
						 UserCleanupService cleanupService)
	{
		this._lifetime = lifetime;
		this._providersConfigurationService = providersConfigurationService;
		this._cleanupService = cleanupService;
	}

	[SlashCommand("check", "Forcefully starts checking for updates in provider", true)]
	public async Task ForceCheckCommand(InteractionContext context, [Option("name", "Update provider name")] string name)
	{
		name = name.Trim();
		BaseUpdateProvider? baseUpdateProvider;
		await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource).ConfigureAwait(false);

		if (this._providersConfigurationService.Providers.TryGetValue(name, out var provider) && provider is BaseUpdateProvider bup)
		{
			baseUpdateProvider = bup;
		}
		else
		{
			var upc = this._providersConfigurationService.Providers.Values.FirstOrDefault(p => p.Name.Where(char.IsUpper).ToString() == name);
			baseUpdateProvider = upc as BaseUpdateProvider;
		}

		if (baseUpdateProvider != null)
		{
			baseUpdateProvider.RestartTimer(TimeSpan.Zero);
			await context.EditResponseAsync(embed: EmbedTemplate.SuccessEmbed("Success")).ConfigureAwait(false);
		}
		else
		{
			await context.EditResponseAsync(embed: EmbedTemplate.ErrorEmbed("Haven't found such update provider")).ConfigureAwait(false);
		}
	}

	[SlashCommand("restart", "Exits bot", true)]
	public async Task StopBotCommand(InteractionContext context)
	{
		await context.CreateResponseAsync("Exiting").ConfigureAwait(false);
		this._lifetime.StopApplication();
	}

	[SlashCommand("cleanup", "Remove users not linked to any guilds", true)]
	public Task CleanupCommand(InteractionContext _)
	{
		return this._cleanupService.ExecuteCleanupAsync();
	}
}