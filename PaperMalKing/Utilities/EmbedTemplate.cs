using System;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace PaperMalKing.Utilities
{
	static class EmbedTemplate
	{
		public static DiscordEmbedBuilder CommandErrorEmbed(Command command, DiscordUser user, Exception ex = null, string message = null)
		{
			var errorMesage = message ?? $"{ex?.Message}\nin\n{Formatter.InlineCode(ex?.Source)}";
			return ErrorEmbed(user, errorMesage, $"Exception occured in {command.Name}");
		}

		public static DiscordEmbedBuilder ErrorEmbed(DiscordUser user, string errorMessage, string title = null)
		{
			var embedBuilder = new DiscordEmbedBuilder
			{
				Author = new DiscordEmbedBuilder.EmbedAuthor
				{
					IconUrl = user.AvatarUrl,
					Name = user.Username
				},
				Title = title ?? "Exception occured",
				Description = errorMessage,
				Timestamp = DateTimeOffset.Now,
				Color = DiscordColor.Red
			};
			return embedBuilder;
		}

		public static DiscordEmbedBuilder SuccessCommand(DiscordUser user, string message)
		{
			var embedBuilder = new DiscordEmbedBuilder
			{
				Author = new DiscordEmbedBuilder.EmbedAuthor
				{
					IconUrl = user.AvatarUrl,
					Name = user.Username
				},
				Title = "Command executed successfully",
				Description = message,
				Timestamp = DateTimeOffset.Now,
				Color = DiscordColor.Green
			};
			return embedBuilder;
		}

	}
}
