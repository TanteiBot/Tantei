// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using PaperMalKing.Common;
using PaperMalKing.UpdatesProviders.Base.Exceptions;

namespace PaperMalKing.UpdatesProviders.Base;

[SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods")]
public abstract class BaseUpdateProviderUserCommandsModule<TUpdateProviderUserService> : ApplicationCommandModule where TUpdateProviderUserService : class, IUpdateProviderUserService
{
	protected ILogger<BaseUpdateProviderUserCommandsModule<TUpdateProviderUserService>> Logger { get; }
	protected TUpdateProviderUserService UserService { get; }

	protected BaseUpdateProviderUserCommandsModule(TUpdateProviderUserService userService, ILogger<BaseUpdateProviderUserCommandsModule<TUpdateProviderUserService>> logger)
	{
		this.UserService = userService;
		this.Logger = logger;
	}

	public virtual async Task AddUserCommand(InteractionContext ctx, string? username = null)
	{
		this.Logger.LogInformation("Trying to add {ProviderUsername} {Member} to {Name} update provider", username, ctx.Member, TUpdateProviderUserService.Name);
		BaseUser user;
		try
		{
			user = await this.UserService.AddUserAsync(ctx.Member!.Id, ctx.Member.Guild.Id, username).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			var embed = ex is UserProcessingException ? EmbedTemplate.ErrorEmbed(ctx, ex.Message) : EmbedTemplate.UnknownErrorEmbed(ctx);
			await ctx.CreateResponseAsync(embed: embed.Build()).ConfigureAwait(false);
			this.Logger.LogError(ex, "Failed to add {ProviderUsername} {Member} to {Name} update provider", username, ctx.Member, TUpdateProviderUserService.Name);
			throw;
		}

		this.Logger.LogInformation("Successfully added {ProviderUsername} {Member} to {Name} update provider", username, ctx.Member, TUpdateProviderUserService.Name);

		await ctx.CreateResponseAsync(embed: EmbedTemplate.SuccessEmbed(ctx,
			$"Successfully added {user.Username} to {TUpdateProviderUserService.Name} update checker")).ConfigureAwait(false);
	}

	public virtual async Task RemoveUserInGuildCommand(InteractionContext ctx)
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
			await ctx.CreateResponseAsync(embed: embed).ConfigureAwait(false);
			this.Logger.LogError(ex, "Failed to remove {Member} from {Name} update provider", ctx.Member, TUpdateProviderUserService.Name);

			throw;
		}
		this.Logger.LogInformation("Successfully removed {Member} from {Name} update provider", ctx.Member, TUpdateProviderUserService.Name);

		await ctx.CreateResponseAsync(embed: EmbedTemplate.SuccessEmbed(ctx,
			$"Successfully removed {user.Username} from {TUpdateProviderUserService.Name} update checker")).ConfigureAwait(false);
	}

	public virtual async Task RemoveUserHereCommand(InteractionContext ctx)
	{
		try
		{
			await this.UserService.RemoveUserHereAsync(ctx.User.Id, ctx.Guild.Id).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			var embed = ex is UserProcessingException ? EmbedTemplate.ErrorEmbed(ctx, ex.Message) : EmbedTemplate.UnknownErrorEmbed(ctx);
			await ctx.CreateResponseAsync(embed: embed).ConfigureAwait(false);
			throw;
		}

		await ctx.CreateResponseAsync(embed: EmbedTemplate.SuccessEmbed(ctx, "Now your updates won't appear in this server")).ConfigureAwait(false);
	}

	public virtual async Task ListUsersCommand(InteractionContext ctx)
	{
		var sb = new StringBuilder();
		try
		{
			var i = 1;
			foreach (var user in this.UserService.ListUsers(ctx.Guild.Id))
			{
				if (sb.Length + user.Username.Length > 2048)
				{
					if (sb.Length + "…".Length > 2048)
						break;

					sb.Append('…');
					break;
				}

				sb.AppendLine(
					$"{i++}. {user.Username} {(user.DiscordUser is null ? "" : Helpers.ToDiscordMention(user.DiscordUser.DiscordUserId))}");
			}
		}
		catch (Exception ex)
		{
			var embed = ex is UserProcessingException ? EmbedTemplate.ErrorEmbed(ctx, ex.Message) : EmbedTemplate.UnknownErrorEmbed(ctx);
			await ctx.CreateResponseAsync(embed: embed).ConfigureAwait(false);
			throw;
		}

		await ctx.CreateResponseAsync(embed: EmbedTemplate.SuccessEmbed(ctx, "Users").WithDescription(sb.ToString())).ConfigureAwait(false);
	}
}