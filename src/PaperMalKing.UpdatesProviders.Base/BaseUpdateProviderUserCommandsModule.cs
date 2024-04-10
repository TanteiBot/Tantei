// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using PaperMalKing.Common;
using PaperMalKing.Database.Models;
using PaperMalKing.UpdatesProviders.Base.Exceptions;

namespace PaperMalKing.UpdatesProviders.Base;

[SuppressMessage("Style", """VSTHRD200:Use "Async" suffix for async methods""", Justification = "This rule does not apply to commands")]
public abstract class BaseUpdateProviderUserCommandsModule<TUpdateProviderUserService, TUser> : BotCommandsModule
	where TUpdateProviderUserService : BaseUpdateProviderUserService<TUser>
	where TUser : class, IUpdateProviderUser
{
	protected ILogger<BaseUpdateProviderUserCommandsModule<TUpdateProviderUserService, TUser>> Logger { get; }

	protected TUpdateProviderUserService UserService { get; }

	protected override bool IsResponseVisibleOnlyForRequester => false;

	protected BaseUpdateProviderUserCommandsModule(TUpdateProviderUserService userService, ILogger<BaseUpdateProviderUserCommandsModule<TUpdateProviderUserService, TUser>> logger)
	{
		this.UserService = userService;
		this.Logger = logger;
	}

	public virtual async Task AddUserCommand(InteractionContext context, string? username = null)
	{
		this.Logger.StartAddingUser(username, context.Member, this.UserService.Name);
		BaseUser user;

		try
		{
			user = await this.UserService.AddUserAsync(context.Member!.Id, context.Member.Guild.Id, username);
		}
		catch (Exception ex)
		{
			var embed = ex is UserProcessingException ? EmbedTemplate.ErrorEmbed(ex.Message) : EmbedTemplate.UnknownErrorEmbed;
			await context.EditResponseAsync(embed: embed);
			this.Logger.FailAddingUser(ex, username, context.Member, this.UserService.Name);
			throw;
		}

		this.Logger.SuccessfullyAddedUser(username, context.Member, this.UserService.Name);

		await context.EditResponseAsync(embed: EmbedTemplate.SuccessEmbed($"Successfully added {user.Username} to {this.UserService.Name} update checker"));
	}

	public virtual async Task RemoveUserInGuildCommand(InteractionContext context)
	{
		this.Logger.StartRemovingUser(context.Member, this.UserService.Name);

		try
		{
			this.UserService.RemoveUser(context.User.Id);
		}
		catch (Exception ex)
		{
			var embed = ex is UserProcessingException ? EmbedTemplate.ErrorEmbed(ex.Message) : EmbedTemplate.UnknownErrorEmbed;
			await context.EditResponseAsync(embed: embed);
			this.Logger.FailRemovingUser(ex, context.Member, this.UserService.Name);

			throw;
		}

		this.Logger.SuccessfullyRemovedUser(context.Member, this.UserService.Name);

		await context.EditResponseAsync(embed: EmbedTemplate.SuccessEmbed($"Successfully removed yourself from {this.UserService.Name} update checker"))
					 ;
	}

	public virtual async Task RemoveUserHereCommand(InteractionContext context)
	{
		try
		{
			await this.UserService.RemoveUserHereAsync(context.User.Id, context.Guild.Id);
		}
		catch (Exception ex)
		{
			var embed = ex is UserProcessingException ? EmbedTemplate.ErrorEmbed(ex.Message) : EmbedTemplate.UnknownErrorEmbed;
			await context.EditResponseAsync(embed: embed);
			throw;
		}

		await context.EditResponseAsync(embed: EmbedTemplate.SuccessEmbed("Now your updates won't appear in this server"));
	}

	public virtual async Task ListUsersCommand(InteractionContext context)
	{
		var sb = new StringBuilder();
		try
		{
			var i = 1;
			foreach (var user in this.UserService.ListUsers(context.Guild.Id))
			{
				if (sb.Length + user.Username.Length > 2048)
				{
					if (sb.Length + "…".Length > 2048)
					{
						break;
					}

					sb.Append('…');
					break;
				}

				sb.AppendLine(
					CultureInfo.InvariantCulture,
					$"{i++}. {user.Username} {(user.DiscordUser is null ? "" : DiscordHelpers.ToDiscordMention(user.DiscordUser.DiscordUserId))}");
			}
		}
		catch (Exception ex)
		{
			var embed = ex is UserProcessingException ? EmbedTemplate.ErrorEmbed(ex.Message) : EmbedTemplate.UnknownErrorEmbed;
			await context.EditResponseAsync(embed: embed);
			throw;
		}

		await context.EditResponseAsync(embed: EmbedTemplate.SuccessEmbed("Users").WithDescription(sb.ToString()));
	}
}