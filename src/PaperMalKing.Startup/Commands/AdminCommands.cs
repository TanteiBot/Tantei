// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Globalization;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Microsoft.Extensions.Hosting;
using PaperMalKing.Common;
using PaperMalKing.Startup.Services;

namespace PaperMalKing.Startup.Commands;

/// <remarks>
/// We don't use <see cref="BotCommandsModule"/> since most commands are immediately executed or dont provide any feedback.
/// </remarks>
[SlashCommandGroup("admin", "Commands for owner")]
[SlashRequireOwner]
[SlashModuleLifespan(SlashModuleLifespan.Singleton)]
internal sealed class AdminCommands(IHostApplicationLifetime _lifetime,
									UpdateProvidersConfigurationService _providersConfigurationService,
									UserCleanupService _cleanupService,
									GuildManagementService _guildManagementService) : ApplicationCommandModule
{
	[SlashCommand("check", "Forcefully starts checking for updates in provider")]
	public async Task ForceCheckCommand(InteractionContext context, [Option(nameof(name), "Update provider name")] string name)
	{
		name = name.Trim();
		await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);

		if (!_providersConfigurationService.Providers.TryGetValue(name, out var baseUpdateProvider))
		{
			baseUpdateProvider = _providersConfigurationService.Providers.Values.FirstOrDefault(p => string.Equals(p.Name.Where(char.IsUpper).ToString(), name, StringComparison.Ordinal));
		}

		if (baseUpdateProvider != null)
		{
			baseUpdateProvider.StartOrRestartAfter(TimeSpan.Zero);
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
	public Task CleanupCommand(InteractionContext _)
	{
		return _cleanupService.ExecuteCleanupAsync();
	}

	[SlashCommand("forceToLeave", "Forces bot to leave from guild")]
	public Task ForceToLeave(InteractionContext _, [Option(nameof(guildId), "Id of guild to leave from")] string guildId)
	{
		return _guildManagementService.RemoveGuildAsync(ulong.Parse(guildId, CultureInfo.InvariantCulture));
	}
}