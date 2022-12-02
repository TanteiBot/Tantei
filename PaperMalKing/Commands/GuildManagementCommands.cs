#region LICENSE

// PaperMalKing.
// Copyright (C) 2021 N0D4N
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Diagnostics.CodeAnalysis;
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
	[Description("Commands for managing server")]
	[ModuleLifespan(ModuleLifespan.Singleton)]
	[OwnerOrPermissions(Permissions.ManageGuild)]
	[SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods")]
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
				await this._managementService.SetChannelAsync(channel.GuildId!.Value, channel.Id).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				var embed = ex is GuildManagementException ? EmbedTemplate.ErrorEmbed(ctx, ex.Message) : EmbedTemplate.UnknownErrorEmbed(ctx);
				await ctx.RespondAsync(embed: embed).ConfigureAwait(false);
				throw;
			}

			await ctx.RespondAsync(embed: EmbedTemplate.SuccessEmbed(ctx, $"Successfully set {channel}")).ConfigureAwait(false);
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
				await this._managementService.UpdateChannelAsync(channel.GuildId!.Value, channel.Id).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				var embed = ex is GuildManagementException ? EmbedTemplate.ErrorEmbed(ctx, ex.Message) : EmbedTemplate.UnknownErrorEmbed(ctx);
				await ctx.RespondAsync(embed: embed).ConfigureAwait(false);
				throw;
			}

			await ctx.RespondAsync(embed: EmbedTemplate.SuccessEmbed(ctx, $"Successfully updated to {channel}")).ConfigureAwait(false);
		}

		[Command("removeserver")]
		[Description("Remove this server from being tracked")]
		public async Task RemoveGuildCommand(CommandContext ctx)
		{
			try
			{
				await this._managementService.RemoveGuildAsync(ctx.Guild.Id).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				var embed = ex is GuildManagementException ? EmbedTemplate.ErrorEmbed(ctx, ex.Message) : EmbedTemplate.UnknownErrorEmbed(ctx);
				await ctx.RespondAsync(embed: embed).ConfigureAwait(false);
				throw;
			}

			await ctx.RespondAsync(embed: EmbedTemplate.SuccessEmbed(ctx, "Successfully removed this server from being tracked"))
					 .ConfigureAwait(false);
		}

		[Command("forseremoveuser")]
		[Aliases("frmu")]
		[Description("Remove this user from being tracked in this server")]
		public async Task ForceRemoveUserCommand(CommandContext ctx, [Description("Discord user's id which should be to removed from being tracked")]
												 ulong userId)
		{
			try
			{
				await this._managementService.RemoveUserAsync(ctx.Guild.Id, userId).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				var embed = ex is GuildManagementException ? EmbedTemplate.ErrorEmbed(ctx, ex.Message) : EmbedTemplate.UnknownErrorEmbed(ctx);
				await ctx.RespondAsync(embed: embed).ConfigureAwait(false);
				throw;
			}

			await ctx.RespondAsync(embed: EmbedTemplate.SuccessEmbed(ctx, $"Successfully removed {userId.ToString()} this server from being tracked"))
					 .ConfigureAwait(false);
		}

		[Command("forseremoveuser")]
		public Task ForceRemoveUserCommand(CommandContext ctx, [Description("Discord user to remove from being tracked")]
										   DiscordUser user) => this.ForceRemoveUserCommand(ctx, user.Id);
	}
}