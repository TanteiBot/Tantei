// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Microsoft.Extensions.Hosting;
using PaperMalKing.Common;
using PaperMalKing.Startup.Services;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;

namespace PaperMalKing.Startup.Commands;

/// <remarks>
/// We don't use <see cref="BotCommandsModule"/> since most commands are immediately executed or dont provide any feedback.
/// </remarks>
[SlashCommandGroup("admin", "Commands for owner")]
[SlashRequireOwner]
[SlashModuleLifespan(SlashModuleLifespan.Singleton)]
[SuppressMessage("Style", """VSTHRD200:Use "Async" suffix for async methods""", Justification = "It doesn't apply to commands")]
internal sealed class AdminCommands(IHostApplicationLifetime _lifetime,
									UpdateProvidersConfigurationService _providersConfigurationService,
									UserCleanupService _cleanupService,
									GuildManagementService _guildManagementService) : ApplicationCommandModule
{
	[SlashCommand("check", "Forcefully starts checking for updates in provider")]
	public async Task ForceCheckCommand(InteractionContext context, [Option(nameof(name), "Update provider name")] string name)
	{
		name = name.Trim();
		BaseUpdateProvider? baseUpdateProvider;
		await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

		if (_providersConfigurationService.Providers.TryGetValue(name, out var provider) && provider is BaseUpdateProvider bup)
		{
			baseUpdateProvider = bup;
		}
		else
		{
			var upc = _providersConfigurationService.Providers.Values.FirstOrDefault(p => string.Equals(p.Name.Where(char.IsUpper).ToString(), name, StringComparison.Ordinal));
			baseUpdateProvider = upc as BaseUpdateProvider;
		}

		if (baseUpdateProvider != null)
		{
			baseUpdateProvider.RestartTimer(TimeSpan.Zero);
			await context.EditResponseAsync(embed: EmbedTemplate.SuccessEmbed("Success"));
		}
		else
		{
			await context.EditResponseAsync(embed: EmbedTemplate.ErrorEmbed("Haven't found such update provider"));
		}
	}

	[SlashCommand("restart", "Exits bot")]
	public async Task StopBotCommand(InteractionContext context)
	{
		await context.CreateResponseAsync("Exiting");
		_lifetime.StopApplication();
	}

	[SlashCommand("cleanup", "Remove users not linked to any guilds")]
	[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "We discard parameter")]
	public Task CleanupCommand(InteractionContext _)
	{
		return _cleanupService.ExecuteCleanupAsync();
	}

	[SlashCommand("forceToLeave", "Forces bot to leave from guild")]
	[SuppressMessage("StyleCop.CSharp.NamingRules", "SA1313:Parameter names should begin with lower-case letter", Justification = "We discard parameter")]
	public Task ForceToLeave(InteractionContext _, [Option(nameof(guildId), "Id of guild to leave from")] string guildId)
	{
		return _guildManagementService.RemoveGuildAsync(ulong.Parse(guildId, CultureInfo.InvariantCulture));
	}
}