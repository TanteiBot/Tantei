//

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Microsoft.Extensions.Logging;
using PaperMalKing.Common;
using PaperMalKing.Common.Attributes;
using PaperMalKing.Exceptions;
using PaperMalKing.Services;

namespace PaperMalKing.Commands;

[SlashCommandGroup("sm", "Commands for managing server", true)]
[SlashModuleLifespan(SlashModuleLifespan.Singleton)]
[OwnerOrPermissions(Permissions.ManageGuild)]
[SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods")]
[GuildOnly, SlashRequireGuild]
public sealed class GuildManagementCommands : ApplicationCommandModule
{
	private readonly ILogger<GuildManagementCommands> _logger;

	private readonly GuildManagementService _managementService;

	public GuildManagementCommands(ILogger<GuildManagementCommands> logger, GuildManagementService managementService)
	{
		this._logger = logger;
		this._managementService = managementService;
	}

	[SlashCommand("set", "Sets channel to post updates to", true)]
	public async Task SetChannelCommand(InteractionContext context,
										[Option("channel", "Channel updates should be posted", autocomplete: false)] DiscordChannel? channel = null)
	{
		await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource).ConfigureAwait(false);

		if (channel is null)
			channel = context.Channel;
		try
		{
			var perms = channel.PermissionsFor(context.Guild.CurrentMember);
			if (!perms.HasPermission(Permissions.SendMessages))
				throw new GuildManagementException(
					$"Bot wouldn't be able to send updates to channel {channel} because it lacks permission to send messages");
			await this._managementService.SetChannelAsync(channel.GuildId!.Value, channel.Id).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			var embed = ex is GuildManagementException ? EmbedTemplate.ErrorEmbed(context, ex.Message) : EmbedTemplate.UnknownErrorEmbed(context);
			await context.EditResponseAsync(embed: embed).ConfigureAwait(false);
			throw;
		}

		await context.EditResponseAsync(embed: EmbedTemplate.SuccessEmbed(context, $"Successfully set {channel}")).ConfigureAwait(false);
	}

	[SlashCommand("update", "Updates channel where updates are posted", true)]
	public async Task UpdateChannelCommand(InteractionContext context,
										   [Option("channel", "New channel where updates should be posted")] DiscordChannel? channel = null)
	{
		await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource).ConfigureAwait(false);
		if (channel is null)
			channel = context.Channel;
		try
		{
			var perms = channel.PermissionsFor(context.Guild.CurrentMember);
			if (!perms.HasPermission(Permissions.SendMessages))
				throw new GuildManagementException(
					$"Bot wouldn't be able to send updates to channel {channel} because it lacks permission to send messages");
			await this._managementService.UpdateChannelAsync(channel.GuildId!.Value, channel.Id).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			var embed = ex is GuildManagementException ? EmbedTemplate.ErrorEmbed(context, ex.Message) : EmbedTemplate.UnknownErrorEmbed(context);
			await context.EditResponseAsync(embed: embed).ConfigureAwait(false);
			throw;
		}

		await context.EditResponseAsync(embed: EmbedTemplate.SuccessEmbed(context, $"Successfully updated to {channel}")).ConfigureAwait(false);
	}

	[SlashCommand("removeserver", "Remove this server from being tracked", true)]
	public async Task RemoveGuildCommand(InteractionContext context)
	{
		await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource).ConfigureAwait(false);
		try
		{
			await this._managementService.RemoveGuildAsync(context.Guild.Id).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			var embed = ex is GuildManagementException ? EmbedTemplate.ErrorEmbed(context, ex.Message) : EmbedTemplate.UnknownErrorEmbed(context);
			await context.EditResponseAsync(embed: embed).ConfigureAwait(false);
			throw;
		}

		await context.EditResponseAsync(embed: EmbedTemplate.SuccessEmbed(context, "Successfully removed this server from being tracked"))
				 .ConfigureAwait(false);
	}

	[SlashCommand("forceremoveuserById", "Remove this user from being tracked in this server", true)]
	public async Task ForceRemoveUserCommand(InteractionContext context,
											 [Option("userId", "Discord user's id which should be to removed from being tracked")] long userId)
	{
		await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource).ConfigureAwait(false);

		try
		{
			await this._managementService.RemoveUserAsync(context.Guild.Id, (ulong)userId).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			var embed = ex is GuildManagementException ? EmbedTemplate.ErrorEmbed(context, ex.Message) : EmbedTemplate.UnknownErrorEmbed(context);
			await context.EditResponseAsync(embed: embed).ConfigureAwait(false);
			throw;
		}

		await context.EditResponseAsync(embed: EmbedTemplate.SuccessEmbed(context, $"Successfully removed {userId} this server from being tracked"))
				 .ConfigureAwait(false);
	}

	[SlashCommand("forceremoveuser", "Remove this user from being tracked in this server")]
	public Task ForceRemoveUserCommand(InteractionContext context, [Option("user", "Discord user to remove from being tracked")] DiscordUser user) =>
		this.ForceRemoveUserCommand(context, (long)user.Id);
}