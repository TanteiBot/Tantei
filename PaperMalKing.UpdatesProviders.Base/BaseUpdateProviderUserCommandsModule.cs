#region LICENSE
// PaperMalKing.
// Copyright (C) 2021-2022 N0D4N
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
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.Logging;
using PaperMalKing.Common;
using PaperMalKing.UpdatesProviders.Base.Exceptions;

namespace PaperMalKing.UpdatesProviders.Base
{
	[SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods")]
	public abstract class BaseUpdateProviderUserCommandsModule<TUpdateProviderUserService> : BaseCommandModule where TUpdateProviderUserService: class, IUpdateProviderUserService
	{
		protected ILogger<BaseUpdateProviderUserCommandsModule<TUpdateProviderUserService>> Logger { get; }
		protected TUpdateProviderUserService UserService { get; }

		protected BaseUpdateProviderUserCommandsModule(TUpdateProviderUserService userService, ILogger<BaseUpdateProviderUserCommandsModule<TUpdateProviderUserService>> logger)
		{
			this.UserService = userService;
			this.Logger = logger;
		}

		public virtual async Task AddUserCommand(CommandContext ctx, [RemainingText] [Description("Your username")]
												 string username)
		{
			this.Logger.LogInformation("Trying to add {ProviderUsername} {Member} to {Name} update provider",username, ctx.Member,  TUpdateProviderUserService.Name);
			BaseUser user;
			try
			{
				user = await this.UserService.AddUserAsync(username, ctx.Member!.Id, ctx.Member.Guild.Id).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				var embed = ex is UserProcessingException ? EmbedTemplate.ErrorEmbed(ctx, ex.Message) : EmbedTemplate.UnknownErrorEmbed(ctx);
				await ctx.RespondAsync(embed: embed.Build()).ConfigureAwait(false);
				this.Logger.LogError(ex,"Failed to add {ProviderUsername} {Member} to {Name} update provider",username, ctx.Member,  TUpdateProviderUserService.Name);
				throw;
			}

			this.Logger.LogInformation("Successfully added {ProviderUsername} {Member} to {Name} update provider",username, ctx.Member,  TUpdateProviderUserService.Name);

			await ctx.RespondAsync(embed: EmbedTemplate.SuccessEmbed(ctx,
				$"Successfully added {user.Username} to {TUpdateProviderUserService.Name} update checker")).ConfigureAwait(false);
		}

		public virtual async Task RemoveUserInGuildCommand(CommandContext ctx)
		{
			this.Logger.LogInformation("Trying to remove {Member} from {Name} update provider", ctx.Member, TUpdateProviderUserService.Name);
			BaseUser user;
			try
			{
				user = await this.UserService.RemoveUserAsync(ctx.User.Id).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				var embed = ex is UserProcessingException ? EmbedTemplate.ErrorEmbed(ctx, ex.Message) : EmbedTemplate.UnknownErrorEmbed(ctx);
				await ctx.RespondAsync(embed: embed).ConfigureAwait(false);
				this.Logger.LogError(ex,"Failed to remove {Member} from {Name} update provider",ctx.Member,  TUpdateProviderUserService.Name);

				throw;
			}
			this.Logger.LogInformation("Successfully removed {Member} from {Name} update provider", ctx.Member, TUpdateProviderUserService.Name);

			await ctx.RespondAsync(embed: EmbedTemplate.SuccessEmbed(ctx,
				$"Successfully removed {user.Username} from {TUpdateProviderUserService.Name} update checker")).ConfigureAwait(false);
		}

		public virtual async Task RemoveUserHereCommand(CommandContext ctx)
		{
			try
			{
				await this.UserService.RemoveUserHereAsync(ctx.User.Id, ctx.Guild.Id).ConfigureAwait(false);
			}
			catch (Exception ex)
			{
				var embed = ex is UserProcessingException ? EmbedTemplate.ErrorEmbed(ctx, ex.Message) : EmbedTemplate.UnknownErrorEmbed(ctx);
				await ctx.RespondAsync(embed: embed).ConfigureAwait(false);
				throw;
			}

			await ctx.RespondAsync(embed: EmbedTemplate.SuccessEmbed(ctx, "Now your updates won't appear in this server")).ConfigureAwait(false);
		}

		public virtual async Task ListUsersCommand(CommandContext ctx)
		{
			var sb = new StringBuilder();
			try
			{
				var i = 1;
				await foreach (var user in this.UserService.ListUsersAsync(ctx.Guild.Id).ConfigureAwait(false))
				{
					if (sb.Length + user.Username.Length > 2048)
					{
						if (sb.Length + "…".Length > 2048)
							break;

						sb.Append("…");
						break;
					}

					sb.AppendLine(
						$"{i++}{user.Username} {(user.DiscordUser == null ? "" : Helpers.ToDiscordMention(user.DiscordUser.DiscordUserId))}");
				}
			}
			catch (Exception ex)
			{
				var embed = ex is UserProcessingException ? EmbedTemplate.ErrorEmbed(ctx, ex.Message) : EmbedTemplate.UnknownErrorEmbed(ctx);
				await ctx.RespondAsync(embed: embed).ConfigureAwait(false);
				throw;
			}

			await ctx.RespondAsync(embed: EmbedTemplate.SuccessEmbed(ctx, "Users").WithDescription(sb.ToString())).ConfigureAwait(false);
		}
	}
}