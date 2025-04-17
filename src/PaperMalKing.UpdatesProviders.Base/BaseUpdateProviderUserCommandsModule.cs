// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2025 N0D4N

using System.Globalization;
using System.Text;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using PaperMalKing.Common;
using PaperMalKing.Database.Models;
using PaperMalKing.UpdatesProviders.Base.Exceptions;

namespace PaperMalKing.UpdatesProviders.Base;

public abstract class BaseUpdateProviderUserCommandsModule<TUpdateProviderUserService, TUser>
	(TUpdateProviderUserService userService, ILogger<BaseUpdateProviderUserCommandsModule<TUpdateProviderUserService, TUser>> logger) : BotCommandsModule
	where TUpdateProviderUserService : BaseUpdateProviderUserService<TUser>
	where TUser : class, IUpdateProviderUser
{
	protected override bool IsResponseVisibleOnlyForRequester => false;

	public virtual async Task AddUserCommand(InteractionContext context, string? username = null)
	{
		using var logScope = CreateLoggerScope(logger, context);

		using var scope = logger.AddUserScope(username);
		logger.StartAddingUser(username, context.Member, userService.Name);
		BaseUser user;

		try
		{
			user = await userService.AddUserAsync(context.Member!.Id, context.Member.Guild.Id, username);
		}
		catch (Exception ex)
		{
			var embed = ex is UserProcessingException ? EmbedTemplate.ErrorEmbed(ex.GetFullMessage()) : EmbedTemplate.UnknownErrorEmbed;
			await context.EditResponseAsync(embed: embed);
			logger.FailAddingUser(ex, username, context.Member, userService.Name);
			throw;
		}

		logger.SuccessfullyAddedUser(username, context.Member, userService.Name);

		await context.EditResponseAsync(embed: EmbedTemplate.SuccessEmbed($"Successfully added {user.Username} to {userService.Name} update checker"));
	}

	public virtual async Task RemoveUserCommand(InteractionContext context)
	{
		using var logScope = CreateLoggerScope(logger, context);

		using var scope = logger.RemoveUserScope(context.Member.DisplayName);
		logger.StartRemovingUser(context.Member, userService.Name);

		try
		{
			userService.RemoveUser(context.User.Id);
		}
		catch (Exception ex)
		{
			var embed = ex is UserProcessingException ? EmbedTemplate.ErrorEmbed(ex.GetFullMessage()) : EmbedTemplate.UnknownErrorEmbed;
			await context.EditResponseAsync(embed: embed);
			logger.FailRemovingUser(ex, context.Member, userService.Name);

			throw;
		}

		logger.SuccessfullyRemovedUser(context.Member, userService.Name);

		await context.EditResponseAsync(embed: EmbedTemplate.SuccessEmbed($"Successfully removed yourself from {userService.Name} update checker"));
	}

	public virtual async Task RemoveUserHereCommand(InteractionContext context)
	{
		using var logScope = CreateLoggerScope(logger, context);
		using var scope = logger.RemoveUserInGuildScope(context.User.Username, context.Guild.Name);
		try
		{
			await userService.RemoveUserHereAsync(context.User.Id, context.Guild.Id);
		}
		catch (Exception ex)
		{
			var embed = ex is UserProcessingException ? EmbedTemplate.ErrorEmbed(ex.GetFullMessage()) : EmbedTemplate.UnknownErrorEmbed;
			await context.EditResponseAsync(embed: embed);
			throw;
		}

		await context.EditResponseAsync(embed: EmbedTemplate.SuccessEmbed("Now your updates won't appear in this server"));
	}

	public virtual async Task ListUsersCommand(InteractionContext context)
	{
		using var logScope = CreateLoggerScope(logger, context);
		const int discordDescriptionLimit = 2048;
		var sb = new StringBuilder();
		try
		{
			var i = 1;
			foreach (var user in userService.ListUsers(context.Guild.Id))
			{
				if (sb.Length + user.Username.Length <= discordDescriptionLimit)
				{
					sb.AppendLine(CultureInfo.InvariantCulture,
						$"{i++}. {user.Username} {(user.DiscordUser is null ? "" : DiscordHelpers.ToDiscordMention(user.DiscordUser.DiscordUserId))}");
				}
				else if (sb.Length + "…".Length <= discordDescriptionLimit)
				{
					sb.Append('…');
				}
				else
				{
					break;
				}
			}
		}
		catch (Exception ex)
		{
			var embed = ex is UserProcessingException ? EmbedTemplate.ErrorEmbed(ex.GetFullMessage()) : EmbedTemplate.UnknownErrorEmbed;
			await context.EditResponseAsync(embed: embed);
			throw;
		}

		await context.EditResponseAsync(embed: EmbedTemplate.SuccessEmbed("Users").WithDescription(sb.ToString()));
	}
}