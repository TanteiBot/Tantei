// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using PaperMalKing.Common;
using PaperMalKing.Database.Models;
using PaperMalKing.UpdatesProviders.Base.Exceptions;

namespace PaperMalKing.UpdatesProviders.Base;

[SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods")]
public abstract class BaseUpdateProviderUserCommandsModule<TUpdateProviderUserService, TUser> : ApplicationCommandModule where TUpdateProviderUserService : BaseUpdateProviderUserService<TUser> where TUser : class, IUpdateProviderUser
{
	protected ILogger<BaseUpdateProviderUserCommandsModule<TUpdateProviderUserService, TUser>> Logger { get; }
	protected TUpdateProviderUserService UserService { get; }

	protected BaseUpdateProviderUserCommandsModule(TUpdateProviderUserService userService, ILogger<BaseUpdateProviderUserCommandsModule<TUpdateProviderUserService, TUser>> logger)
	{
		this.UserService = userService;
		this.Logger = logger;
	}

	public virtual async Task AddUserCommand(InteractionContext context, string? username = null)
	{
		this.Logger.LogInformation("Trying to add {ProviderUsername} {Member} to {Name} update provider", username, context.Member, UserService.Name);
		BaseUser user;
		await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource).ConfigureAwait(false);

		try
		{
			user = await this.UserService.AddUserAsync(context.Member!.Id, context.Member.Guild.Id, username).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			var embed = ex is UserProcessingException ? EmbedTemplate.ErrorEmbed(context, ex.Message) : EmbedTemplate.UnknownErrorEmbed(context);
			await context.EditResponseAsync(embed: embed.Build()).ConfigureAwait(false);
			this.Logger.LogError(ex, "Failed to add {ProviderUsername} {Member} to {Name} update provider", username, context.Member, UserService.Name);
			throw;
		}

		this.Logger.LogInformation("Successfully added {ProviderUsername} {Member} to {Name} update provider", username, context.Member, UserService.Name);

		await context.EditResponseAsync(embed: EmbedTemplate.SuccessEmbed(context,
			$"Successfully added {user.Username} to {UserService.Name} update checker")).ConfigureAwait(false);
	}

	public virtual async Task RemoveUserInGuildCommand(InteractionContext context)
	{
		this.Logger.LogInformation("Trying to remove {Member} from {Name} update provider", context.Member, UserService.Name);
		await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource).ConfigureAwait(false);

		try
		{
			await this.UserService.RemoveUserAsync(context.User.Id).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			var embed = ex is UserProcessingException ? EmbedTemplate.ErrorEmbed(context, ex.Message) : EmbedTemplate.UnknownErrorEmbed(context);
			await context.EditResponseAsync(embed: embed).ConfigureAwait(false);
			this.Logger.LogError(ex, "Failed to remove {Member} from {Name} update provider", context.Member, UserService.Name);

			throw;
		}
		this.Logger.LogInformation("Successfully removed {Member} from {Name} update provider", context.Member, UserService.Name);

		await context.EditResponseAsync(embed: EmbedTemplate.SuccessEmbed(context,
			$"Successfully removed yourself from {UserService.Name} update checker")).ConfigureAwait(false);
	}

	public virtual async Task RemoveUserHereCommand(InteractionContext context)
	{
		await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource).ConfigureAwait(false);
		try
		{
			await this.UserService.RemoveUserHereAsync(context.User.Id, context.Guild.Id).ConfigureAwait(false);
		}
		catch (Exception ex)
		{
			var embed = ex is UserProcessingException ? EmbedTemplate.ErrorEmbed(context, ex.Message) : EmbedTemplate.UnknownErrorEmbed(context);
			await context.EditResponseAsync(embed: embed).ConfigureAwait(false);
			throw;
		}

		await context.EditResponseAsync(embed: EmbedTemplate.SuccessEmbed(context, "Now your updates won't appear in this server")).ConfigureAwait(false);
	}

	public virtual async Task ListUsersCommand(InteractionContext context)
	{
		await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource).ConfigureAwait(false);
		var sb = new StringBuilder();
		try
		{
			var i = 1;
			foreach (var user in this.UserService.ListUsers(context.Guild.Id))
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
			var embed = ex is UserProcessingException ? EmbedTemplate.ErrorEmbed(context, ex.Message) : EmbedTemplate.UnknownErrorEmbed(context);
			await context.EditResponseAsync(embed: embed).ConfigureAwait(false);
			throw;
		}

		await context.EditResponseAsync(embed: EmbedTemplate.SuccessEmbed(context, "Users").WithDescription(sb.ToString())).ConfigureAwait(false);
	}
}