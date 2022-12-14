// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2022 N0D4N

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Microsoft.Extensions.Hosting;
using PaperMalKing.Common;
using PaperMalKing.Services;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;
using Polly;

namespace PaperMalKing.Commands;

[SlashCommandGroup("admin", "Commands for owner")]
[SlashRequireOwner]
[SlashModuleLifespan(SlashModuleLifespan.Singleton)]
[SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods")]
public sealed class AdminCommands : ApplicationCommandModule
{
	private readonly UpdateProvidersConfigurationService _providersConfigurationService;
	private readonly IHostApplicationLifetime _lifetime;

	public AdminCommands(IHostApplicationLifetime lifetime, UpdateProvidersConfigurationService providersConfigurationService)
	{
		this._lifetime = lifetime;
		this._providersConfigurationService = providersConfigurationService;
	}

	[SlashCommand("check", "Forcefully starts checking for updates in provider", true)]
	public async Task ForceCheckCommand(InteractionContext context, [Option("name", "Update provider name")] string name)
	{
		name = name.Trim();
		BaseUpdateProvider? baseUpdateProvider;
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
			await context.CreateResponseAsync(embed: EmbedTemplate.SuccessEmbed(context, "Success")).ConfigureAwait(false);
		}
		else
		{
			await context.CreateResponseAsync(embed: EmbedTemplate.ErrorEmbed(context, "Haven't found such update provider")).ConfigureAwait(false);
		}
	}

	[SlashCommand("restart", "Exits bot", true)]
	public async Task StopBotCommand(InteractionContext context)
	{
		await context.CreateResponseAsync("Exiting").ConfigureAwait(false);
		this._lifetime.StopApplication();
	}
}