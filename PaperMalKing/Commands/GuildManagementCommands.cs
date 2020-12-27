using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using PaperMalKing.Common;
using PaperMalKing.Common.Attributes;
using PaperMalKing.Exceptions;
using PaperMalKing.Services;

namespace PaperMalKing.Commands
{
	[Group("ServerManagement")]
	[Aliases("sm", "srvmgm")]
	[ModuleLifespan(ModuleLifespan.Singleton)]
	[OwnerOrPermission(Permissions.ManageGuild)]
	public sealed class GuildManagementCommands : BaseCommandModule
	{
		private readonly ILogger<GuildManagementCommands> _logger;

		private readonly GuildManagementService _managementService;

		/// <inheritdoc />
		public GuildManagementCommands(ILogger<GuildManagementCommands> logger, GuildManagementService managementService)
		{
			this._logger = logger;
			this._managementService = managementService;
		}

		[Command("set")]
		[Aliases("s", "st")]
		[Description("Sets channel to post updates to")]
		public async Task SetChannelCommand(CommandContext ctx, [Description("Channel updates should be posted")]
											DiscordChannel? channel = null)
		{
			if (channel == null)
				channel = ctx.Channel;
			try
			{
				var perms = channel.PermissionsFor(ctx.Guild.CurrentMember);
				if (!perms.HasPermission(Permissions.SendMessages))
					throw new GuildManagementException(
						$"Bot wouldn't be able to send updates to channel {channel} because it lacks permission to send messages");
				await this._managementService.SetChannelAsync( channel.GuildId, channel.Id);
			}
			catch (Exception ex)
			{
				var embed = ex is GuildManagementException ? EmbedTemplate.ErrorEmbed(ctx, ex.Message) : EmbedTemplate.UnknownErrorEmbed(ctx);
				await ctx.RespondAsync(embed: embed);
				throw;
			}

			await ctx.RespondAsync(embed: EmbedTemplate.SuccessEmbed(ctx, $"Successfully set {channel}"));
		}

		[Command("update")]
		[Aliases("u")]
		[Description("Updates channel where updates are posted")]
		public async Task UpdateChannelCommand(CommandContext ctx, [Description("New channel where updates should be posted")]
											   DiscordChannel? channel = null)
		{
			if (channel == null)
				channel = ctx.Channel;
			try
			{
				var perms = channel.PermissionsFor(ctx.Guild.CurrentMember);
				if (!perms.HasPermission(Permissions.SendMessages))
					throw new GuildManagementException(
						$"Bot wouldn't be able to send updates to channel {channel} because it lacks permission to send messages");
				await this._managementService.UpdateChannelAsync(channel.GuildId, channel.Id);
			}
			catch (Exception ex)
			{
				var embed = ex is GuildManagementException ? EmbedTemplate.ErrorEmbed(ctx, ex.Message) : EmbedTemplate.UnknownErrorEmbed(ctx);
				await ctx.RespondAsync(embed: embed);
				throw;
			}

			await ctx.RespondAsync(embed: EmbedTemplate.SuccessEmbed(ctx, $"Successfully updated to {channel}"));
		}

		[Command("removeserver")]
		[Description("Remove this server from being tracked")]
		public async Task RemoveGuildCommand(CommandContext ctx)
		{
			try
			{
				await this._managementService.RemoveGuildAsync(ctx.Guild.Id);
			}
			catch (Exception ex)
			{
				var embed = ex is GuildManagementException ? EmbedTemplate.ErrorEmbed(ctx, ex.Message) : EmbedTemplate.UnknownErrorEmbed(ctx);
				await ctx.RespondAsync(embed: embed);
				throw;
			}

			await ctx.RespondAsync(embed: EmbedTemplate.SuccessEmbed(ctx, "Successfully removed this server from being tracked"));
		}

		[Command("forseremoveuser")]
		[Aliases("frmu")]
		[Description("Remove this user from being tracked in this server")]
		public async Task ForceRemoveUserCommand(CommandContext ctx, [Description("Discord user's id which should be to removed from being tracked")]
												 ulong userId)
		{
			try
			{
				await this._managementService.RemoveUserAsync(ctx.Guild.Id, userId);
			}
			catch (Exception ex)
			{
				var embed = ex is GuildManagementException ? EmbedTemplate.ErrorEmbed(ctx, ex.Message) : EmbedTemplate.UnknownErrorEmbed(ctx);
				await ctx.RespondAsync(embed: embed);
				throw;
			}

			await ctx.RespondAsync(embed: EmbedTemplate.SuccessEmbed(ctx, $"Successfully removed {userId.ToString()} this server from being tracked"));
		}

		[Command("forseremoveuser")]
		public Task ForceRemoveUserCommand(CommandContext ctx, [Description("Discord user to remove from being tracked")]
										   DiscordUser user) => this.ForceRemoveUserCommand(ctx, user.Id);
	}
}