using System;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using PaperMalKing.Attributes;

namespace PaperMalKing.Commands
{
	public sealed class UngruppedCommands : BaseCommandModule
	{
		[Command("say")]
		[Description("Sends embed in selected channel with selected text")]
		[OwnerOrPermission(Permissions.ManageGuild)]
		public async Task Say(CommandContext context,
		[Description("Channel where the embed will be send")]DiscordChannel channelToSayIn,
		[RemainingText, Description("Text to send")] string messageContent)
		{
			if (string.IsNullOrWhiteSpace(messageContent))
				throw new ArgumentException("Message's content shouldn't be empty", nameof(messageContent));
			var embed = new DiscordEmbedBuilder
			{
				Description = messageContent.Replace("@everyone", "@\u200beveryone").Replace("@here", "@\u200bhere"),
				Timestamp = DateTime.Now,
				Color = DiscordColor.Blue,
			}.WithAuthor($"{context.Member.Username}#{context.Member.Discriminator}", iconUrl: context.Member.AvatarUrl);
			try
			{
				await channelToSayIn.SendMessageAsync(embed: embed);
			}
			catch
			{
				await context.RespondAsync("Couldn't send message. Check permissions for bot and try again.");
			}
		}

		[Command("About")]
		[Description("Displays info about bot")]
		[Aliases("Info")]
		public async Task About(CommandContext context)
		{
			var botVersion = Assembly.GetEntryAssembly()?.GetName().Version.ToString(3) ?? "";
			var netCoreVersion = Environment.Version.ToString(3);

			var desc =
$@"Paper Mal King is bot designed to automatically track  its users updates on MyAnimeList.

Bot version - {botVersion}.
.NET Core version - {netCoreVersion}.";

			var links = Formatter.MaskedUrl("Source code", new Uri("https://github.com/N0D4N",UriKind.Absolute))+
						"\n";

			var embedBuilder = new DiscordEmbedBuilder
			{
				Title = "Info",
				Description = desc,
				Timestamp = DateTimeOffset.Now,
				Color = DiscordColor.DarkBlue,
				ThumbnailUrl = context.Client.CurrentUser.AvatarUrl
			};
			embedBuilder.AddField("Links", links, true);

			await context.RespondAsync(embed: embedBuilder.Build());
		}


	}
}
