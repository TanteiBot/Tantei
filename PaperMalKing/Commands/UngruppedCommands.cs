using System;
using System.Linq;
using System.Reflection;
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
		public async Task SayCommand(CommandContext context,
			[Description("Channel where the embed will be send")]
			DiscordChannel channelToSayIn,
			[RemainingText, Description("Text to send")]
			string messageContent)
		{
			if (string.IsNullOrWhiteSpace(messageContent))
				throw new ArgumentException("Message's content shouldn't be empty", nameof(messageContent));
			var embed = new DiscordEmbedBuilder
			{
				Description = messageContent.Replace("@everyone", "@\u200beveryone").Replace("@here", "@\u200bhere"),
				Timestamp = DateTime.Now,
				Color = DiscordColor.Blue,
			}.WithAuthor($"{context.Member.Username}#{context.Member.Discriminator}",
				iconUrl: context.Member.AvatarUrl);
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
		public async Task AboutCommand(CommandContext context)
		{
			var botVersion = Assembly.GetEntryAssembly()?.GetName().Version.ToString(3) ?? "";
			var netCoreVersion = Environment.Version.ToString(3);

			var desc =
				"Paper Mal King is bot designed to automatically track  its users updates on MyAnimeList.\nDeveloped by N0D4N#2281 (<@356518417987141633>).";

			var versions = $"Bot version - {botVersion}." +
			               "\n" +
			               $"DSharpPlus version - {context.Client.VersionString}." +
			               "\n" +
			               $".NET Core version - {netCoreVersion}.";

			var sourceCodeLink = "https://github.com/N0D4N/PaperMalKing";
			var links = Formatter.MaskedUrl("Source code", new Uri(sourceCodeLink, UriKind.Absolute)) +
			            "\n" +
			            Formatter.MaskedUrl("Wiki",
				            new Uri("https://github.com/N0D4N/PaperMalKing/wiki", UriKind.Absolute));

			var embedBuilder = new DiscordEmbedBuilder
			{
				Title = "Info",
				Url = sourceCodeLink,
				Description = desc,
				Timestamp = DateTimeOffset.Now,
				Color = DiscordColor.DarkBlue,
				ThumbnailUrl = context.Client.CurrentUser.AvatarUrl
			};
			embedBuilder.AddField("Links", links, true);
			embedBuilder.AddField("Versions", versions, true);

			await context.RespondAsync(embed: embedBuilder.Build());
		}

		[Command("DeleteMessages")]
		[Aliases("dmsg")]
		[RequireOwner]
		public async Task DeleteMessagesCommand(CommandContext context, [RemainingText, Description("Messages Id's")]
			params ulong[] messages)
		{
			var msgsToDelete =
				(await context.Channel.GetMessagesBeforeAsync(context.Message.Id)).Where(x =>
					x.Author.IsCurrent && messages.Contains(x.Id));
			foreach (var msg in msgsToDelete)
				await context.Channel.DeleteMessageAsync(msg);
		}

		[Command("Exit")]
		[RequireOwner]
		public Task ExitCommand(CommandContext context)
		{
			context.Client.DebugLogger.LogMessage(LogLevel.Error, context.User.Username, "Exiting due to command", DateTime.UtcNow);
			Environment.Exit(0);
			return Task.CompletedTask;
		}
	}
}