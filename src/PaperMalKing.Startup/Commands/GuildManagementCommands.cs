// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using PaperMalKing.Common;
using PaperMalKing.Common.Attributes;
using PaperMalKing.Startup.Exceptions;
using PaperMalKing.Startup.Services;
using PaperMalKing.UpdatesProviders.Base;

namespace PaperMalKing.Startup.Commands;

[SlashCommandGroup("sm", "Commands for managing server")]
[SlashModuleLifespan(SlashModuleLifespan.Singleton)]
[OwnerOrPermissions(Permissions.ManageGuild)]
[SuppressMessage("Style", """VSTHRD200:Use "Async" suffix for async methods""", Justification = "It doesn't apply to commands")]
[GuildOnly]
[SlashRequireGuild]
internal sealed class GuildManagementCommands(GuildManagementService _managementService, GeneralUserService _userService) : BotCommandsModule
{
	protected override bool IsResponseVisibleOnlyForRequester => false;

	[SlashCommand("set", "Sets channel to post updates to")]
	public async Task SetChannelCommand(InteractionContext context, [Option(nameof(channel), "Channel updates should be posted", autocomplete: false)] DiscordChannel? channel = null)
	{
		channel ??= context.Channel;
		if (channel.IsCategory || channel.IsThread)
		{
			await context.EditResponseAsync(embed: EmbedTemplate.ErrorEmbed("You cant set posting channel to category or to a thread"));
			return;
		}

		try
		{
			var perms = channel.PermissionsFor(context.Guild.CurrentMember);
			if (!perms.HasPermission(Permissions.SendMessages))
			{
				await context.EditResponseAsync(embed: EmbedTemplate.ErrorEmbed(
								 $"Bot wouldn't be able to send updates to channel {channel} because it lacks permission to send messages",
								 "Permissions error"));
			}

			await _managementService.SetChannelAsync(channel.GuildId!.Value, channel.Id);
		}
		catch (Exception ex)
		{
			var embed = ex is GuildManagementException ? EmbedTemplate.ErrorEmbed(ex.GetFullMessage()) : EmbedTemplate.UnknownErrorEmbed;
			await context.EditResponseAsync(embed: embed);
			throw;
		}

		await context.EditResponseAsync(embed: EmbedTemplate.SuccessEmbed($"Successfully set {channel}"));
	}

	[SlashCommand("update", "Updates channel where updates are posted")]
	public async Task UpdateChannelCommand(InteractionContext context, [Option(nameof(channel), "New channel where updates should be posted")] DiscordChannel? channel = null)
	{
		channel ??= context.Channel;
		if (channel.IsCategory || channel.IsThread)
		{
			await context.EditResponseAsync(EmbedTemplate.ErrorEmbed("You cant set posting channel to category or to a thread"));
			return;
		}

		try
		{
			var perms = channel.PermissionsFor(context.Guild.CurrentMember);
			if (!perms.HasPermission(Permissions.SendMessages))
			{
				await context.EditResponseAsync(embed: EmbedTemplate.ErrorEmbed(
								 $"Bot wouldn't be able to send updates to channel {channel} because it lacks permission to send messages",
								 "Permissions error"));
			}

			await _managementService.UpdateChannelAsync(channel.GuildId!.Value, channel.Id);
		}
		catch (Exception ex)
		{
			var embed = ex is GuildManagementException ? EmbedTemplate.ErrorEmbed(ex.GetFullMessage()) : EmbedTemplate.UnknownErrorEmbed;
			await context.EditResponseAsync(embed: embed);
			throw;
		}

		await context.EditResponseAsync(embed: EmbedTemplate.SuccessEmbed($"Successfully updated to {channel}"));
	}

	[SlashCommand("removeserver", "Remove this server from being tracked")]
	public async Task RemoveGuildCommand(InteractionContext context)
	{
		try
		{
			await _managementService.RemoveGuildAsync(context.Guild.Id);
		}
		catch (Exception ex)
		{
			var embed = ex is GuildManagementException ? EmbedTemplate.ErrorEmbed(ex.GetFullMessage()) : EmbedTemplate.UnknownErrorEmbed;
			await context.EditResponseAsync(embed: embed);
			throw;
		}

		await context.EditResponseAsync(embed: EmbedTemplate.SuccessEmbed("Successfully removed this server from being tracked"));
	}

	[SlashCommand("forceremoveuserById", "Remove this user from being tracked in this server")]
	public async Task ForceRemoveUserCommand(InteractionContext context, [Option(nameof(userId), "Discord user's id which should be to removed from being tracked")] long userId)
	{
		try
		{
			await _userService.RemoveUserInGuildAsync(context.Guild.Id, (ulong)userId);
		}
		catch (Exception ex)
		{
			var embed = ex is GuildManagementException ? EmbedTemplate.ErrorEmbed(ex.GetFullMessage()) : EmbedTemplate.UnknownErrorEmbed;
			await context.EditResponseAsync(embed: embed);
			throw;
		}

		await context.EditResponseAsync(embed: EmbedTemplate.SuccessEmbed(string.Create(
			CultureInfo.InvariantCulture,
			$"Successfully removed {userId} this server from being tracked")));
	}

	[SlashCommand("forceremoveuser", "Remove this user from being tracked in this server")]
	public Task ForceRemoveUserCommand(InteractionContext context, [Option(nameof(user), "Discord user to remove from being tracked")] DiscordUser user) => this.ForceRemoveUserCommand(context, (long)user.Id);
}