//

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Net;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using PaperMalKing.Common;
using PaperMalKing.Common.Attributes;
using PaperMalKing.Exceptions;
using PaperMalKing.Services;

namespace PaperMalKing.Commands;

[SlashCommandGroup("ServerManagement", "Commands for managing server", true)]
[SlashModuleLifespan(SlashModuleLifespan.Singleton)]
[OwnerOrPermissions(Permissions.ManageGuild)]
[SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods")]
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
	public async Task SetChannelCommand(InteractionContext ctx,
										[Option("channel", "Channel updates should be posted", autocomplete: false)] DiscordChannel? channel = null)
	{
		if (channel is null)
			channel = ctx.Channel;
		try
		{
			var perms = channel.PermissionsFor(ctx.Guild.CurrentMember);
			if (!perms.HasPermission(Permissions.SendMessages))
				throw new GuildManagementException(
					$"Bot wouldn't be able to send updates to channel {channel} because it lacks permission to send messages");
			await this._managementService.SetChannelAsync(channel.GuildId!.Value, channel.Id).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			var embed = ex is GuildManagementException ? EmbedTemplate.ErrorEmbed(ctx, ex.Message) : EmbedTemplate.UnknownErrorEmbed(ctx);
			await ctx.CreateResponseAsync(embed: embed).ConfigureAwait(false);
			throw;
		}

		await ctx.CreateResponseAsync(embed: EmbedTemplate.SuccessEmbed(ctx, $"Successfully set {channel}")).ConfigureAwait(false);
	}

	[SlashCommand("update", "Updates channel where updates are posted", true)]
	public async Task UpdateChannelCommand(InteractionContext ctx,
										   [Option("channel", "New channel where updates should be posted")] DiscordChannel? channel = null)
	{
		if (channel is null)
			channel = ctx.Channel;
		try
		{
			var perms = channel.PermissionsFor(ctx.Guild.CurrentMember);
			if (!perms.HasPermission(Permissions.SendMessages))
				throw new GuildManagementException(
					$"Bot wouldn't be able to send updates to channel {channel} because it lacks permission to send messages");
			await this._managementService.UpdateChannelAsync(channel.GuildId!.Value, channel.Id).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			var embed = ex is GuildManagementException ? EmbedTemplate.ErrorEmbed(ctx, ex.Message) : EmbedTemplate.UnknownErrorEmbed(ctx);
			await ctx.CreateResponseAsync(embed: embed).ConfigureAwait(false);
			throw;
		}

		await ctx.CreateResponseAsync(embed: EmbedTemplate.SuccessEmbed(ctx, $"Successfully updated to {channel}")).ConfigureAwait(false);
	}

	[SlashCommand("removeserver", "Remove this server from being tracked", true)]
	public async Task RemoveGuildCommand(InteractionContext ctx)
	{
		try
		{
			await this._managementService.RemoveGuildAsync(ctx.Guild.Id).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			var embed = ex is GuildManagementException ? EmbedTemplate.ErrorEmbed(ctx, ex.Message) : EmbedTemplate.UnknownErrorEmbed(ctx);
			await ctx.CreateResponseAsync(embed: embed).ConfigureAwait(false);
			throw;
		}

		await ctx.CreateResponseAsync(embed: EmbedTemplate.SuccessEmbed(ctx, "Successfully removed this server from being tracked"))
				 .ConfigureAwait(false);
	}

	[SlashCommand("forceremoveuserById", "Remove this user from being tracked in this server", true)]
	public async Task ForceRemoveUserCommand(InteractionContext ctx,
											 [Option("userId", "Discord user's id which should be to removed from being tracked")] long userId)
	{
		try
		{
			await this._managementService.RemoveUserAsync(ctx.Guild.Id, (ulong)userId).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			var embed = ex is GuildManagementException ? EmbedTemplate.ErrorEmbed(ctx, ex.Message) : EmbedTemplate.UnknownErrorEmbed(ctx);
			await ctx.CreateResponseAsync(embed: embed).ConfigureAwait(false);
			throw;
		}

		await ctx.CreateResponseAsync(embed: EmbedTemplate.SuccessEmbed(ctx, $"Successfully removed {userId} this server from being tracked"))
				 .ConfigureAwait(false);
	}

	[SlashCommand("forceremoveuser", "Remove this user from being tracked in this server")]
	public Task ForceRemoveUserCommand(InteractionContext ctx, [Option("user", "Discord user to remove from being tracked")] DiscordUser user) =>
		this.ForceRemoveUserCommand(ctx, (long)user.Id);
}