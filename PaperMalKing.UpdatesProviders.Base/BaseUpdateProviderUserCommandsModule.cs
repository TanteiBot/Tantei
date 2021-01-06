using System;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.Logging;
using PaperMalKing.Common;
using PaperMalKing.UpdatesProviders.Base.Exceptions;

namespace PaperMalKing.UpdatesProviders.Base
{
	public class BaseUpdateProviderUserCommandsModule : BaseCommandModule
	{
		protected readonly ILogger<BaseUpdateProviderUserCommandsModule> Logger;
		protected readonly IUpdateProviderUserService UserService;

		/// <inheritdoc />
		public BaseUpdateProviderUserCommandsModule(IUpdateProviderUserService userService, ILogger<BaseUpdateProviderUserCommandsModule> logger)
		{
			this.UserService = userService;
			this.Logger = logger;
		}

		public virtual async Task AddUserCommand(CommandContext ctx, [RemainingText] [Description("Your username")]
												 string username)
		{
			this.Logger.LogInformation("Trying to add {ProviderUsername} {Member} to {Name} update provider",username, ctx.Member,  UserService.Name);
			BaseUser user;
			try
			{
				user = await this.UserService.AddUserAsync(username, ctx.Member.Id, ctx.Member.Guild.Id);
			}
			catch (Exception ex)
			{
				var embed = ex is UserProcessingException ? EmbedTemplate.ErrorEmbed(ctx, ex.Message) : EmbedTemplate.UnknownErrorEmbed(ctx);
				await ctx.RespondAsync(embed: embed.Build());
				this.Logger.LogError(ex,"Failed to add {ProviderUsername} {Member} to {Name} update provider",username, ctx.Member,  UserService.Name);
				throw;
			}

			this.Logger.LogInformation("Successfully added {ProviderUsername} {Member} to {Name} update provider",username, ctx.Member,  UserService.Name);

			await ctx.RespondAsync(embed: EmbedTemplate.SuccessEmbed(ctx,
				$"Successfully added {user.Username} to {this.UserService.Name} update checker"));
		}


		public virtual async Task RemoveUserInGuildCommand(CommandContext ctx)
		{
			this.Logger.LogInformation("Trying to remove {Member} from {Name} update provider", ctx.Member, UserService.Name);
			BaseUser user;
			try
			{
				user = await this.UserService.RemoveUserAsync(ctx.User.Id);
			}
			catch (Exception ex)
			{
				var embed = ex is UserProcessingException ? EmbedTemplate.ErrorEmbed(ctx, ex.Message) : EmbedTemplate.UnknownErrorEmbed(ctx);
				await ctx.RespondAsync(embed: embed);
				this.Logger.LogError(ex,"Failed to remove {Member} from {Name} update provider",ctx.Member,  UserService.Name);

				throw;
			}
			this.Logger.LogInformation("Successfully removed {Member} from {Name} update provider", ctx.Member, UserService.Name);

			await ctx.RespondAsync(embed: EmbedTemplate.SuccessEmbed(ctx,
				$"Successfully removed {user.Username} from {this.UserService.Name} update checker"));
		}

		public virtual async Task RemoveUserHereCommand(CommandContext ctx)
		{
			try
			{
				await this.UserService.RemoveUserHereAsync(ctx.User.Id, ctx.Guild.Id);
			}
			catch (Exception ex)
			{
				var embed = ex is UserProcessingException ? EmbedTemplate.ErrorEmbed(ctx, ex.Message) : EmbedTemplate.UnknownErrorEmbed(ctx);
				await ctx.RespondAsync(embed: embed);
				throw;
			}

			await ctx.RespondAsync(embed: EmbedTemplate.SuccessEmbed(ctx, $"Now your updates won't appear in this server"));
		}

		public virtual async Task ListUsersCommand(CommandContext ctx)
		{
			var sb = new StringBuilder();
			try
			{
				var i = 1;
				await foreach (var user in this.UserService.ListUsersAsync(ctx.Guild.Id))
				{
					if (sb.Length + user.Username.Length > 2048)
					{
						if (sb.Length + "...".Length > 2048)
							break;

						sb.Append("...");
						break;
					}

					sb.AppendLine(
						$"{i++.ToString()}{user.Username} {(user.DiscordUser == null ? "" : Helpers.ToDiscordMention(user.DiscordUser.DiscordUserId))}");
				}
			}
			catch (Exception ex)
			{
				var embed = ex is UserProcessingException ? EmbedTemplate.ErrorEmbed(ctx, ex.Message) : EmbedTemplate.UnknownErrorEmbed(ctx);
				await ctx.RespondAsync(embed: embed);
				throw;
			}

			await ctx.RespondAsync(embed: EmbedTemplate.SuccessEmbed(ctx, "Users").WithDescription(sb.ToString()));
		}
	}
}