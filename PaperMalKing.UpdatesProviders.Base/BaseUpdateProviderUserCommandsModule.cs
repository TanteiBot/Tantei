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
			IUser user;
			try
			{
				user = await this.UserService.AddUserAsync(username, ctx.Member.Id, ctx.Member.Guild.Id);
			}
			catch (Exception ex)
			{
				var embed = ex is UserProcessingException ? EmbedTemplate.ErrorEmbed(ctx, ex.Message) : EmbedTemplate.UnknownErrorEmbed(ctx);
				await ctx.RespondAsync(embed: embed);
				throw;
			}

			await ctx.RespondAsync(embed: EmbedTemplate.SuccessEmbed(ctx,
				$"Successfully added {user.Username} to {this.UserService.Name} update checker"));
		}


		public virtual async Task RemoveUserInGuildCommand(CommandContext ctx)
		{
			IUser user;
			try
			{
				user = await this.UserService.RemoveUserAsync(ctx.User.Id);
			}
			catch (Exception ex)
			{
				var embed = ex is UserProcessingException ? EmbedTemplate.ErrorEmbed(ctx, ex.Message) : EmbedTemplate.UnknownErrorEmbed(ctx);
				await ctx.RespondAsync(embed: embed);
				throw;
			}

			await ctx.RespondAsync(embed: EmbedTemplate.SuccessEmbed(ctx,
				$"Successfully removed {user.Username} from {this.UserService.Name} update checker"));
		}

		public virtual async Task ListUsersCommand(CommandContext ctx)
		{
			var sb = new StringBuilder();
			try
			{
				await foreach (var user in this.UserService.ListUsersAsync(ctx.Guild.Id))
				{
					var i = 1;
					if (sb.Length + user.Username.Length > 2048)
					{
						if (sb.Length + "...".Length > 2048)
							break;

						sb.Append("...");
						break;
					}

					sb.AppendLine($"{i++.ToString()}{user.Username}");
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