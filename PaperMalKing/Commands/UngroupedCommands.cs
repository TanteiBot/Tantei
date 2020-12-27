using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using PaperMalKing.Common;
using PaperMalKing.Common.Attributes;
using PaperMalKing.Services;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;

namespace PaperMalKing.Commands
{
	[ModuleLifespan(ModuleLifespan.Singleton)]
	public sealed class UngroupedCommands : BaseCommandModule
	{
		private readonly UpdateProvidersConfigurationService _providersConfigurationService;

		/// <inheritdoc />
		public UngroupedCommands(UpdateProvidersConfigurationService providersConfigurationService)
		{
			this._providersConfigurationService = providersConfigurationService;
		}

		[Command("say")]
		[Description("Sends embed in selected channel with selected text")]
		[OwnerOrPermission(Permissions.ManageGuild)]
		public async Task SayCommand(CommandContext context, [Description("Channel where the embed will be send")]
									 DiscordChannel channelToSayIn, [RemainingText, Description("Text to send")]
									 string messageContent)
		{
			if (string.IsNullOrWhiteSpace(messageContent))
				throw new ArgumentException("Message's content shouldn't be empty", nameof(messageContent));
			var embed = new DiscordEmbedBuilder
			{
				Description = messageContent.Replace("@everyone", "@\u200beveryone").Replace("@here", "@\u200bhere"),
				Timestamp = DateTime.Now,
				Color = DiscordColor.Blue
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
		public async Task AboutCommand(CommandContext context)
		{
			var botVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString(3) ?? "";
			var netCoreVersion = Environment.Version.ToString(3);

			const string desc =
				"Paper Mal King is bot designed to automatically track  its users updates on MyAnimeList.\nDeveloped by N0D4N#2281 (<@356518417987141633>).";

			var versions = $"Bot version - {botVersion}." + "\n" + $"DSharpPlus version - {context.Client.VersionString}." + "\n" +
						   $".NET Core version - {netCoreVersion}.";

			const string sourceCodeLink = "https://github.com/N0D4N/PaperMalKing";
			var links = Formatter.MaskedUrl("Source code", new Uri(sourceCodeLink, UriKind.Absolute)) + "\n" +
						Formatter.MaskedUrl("Wiki", new Uri("https://github.com/N0D4N/PaperMalKing/wiki", UriKind.Absolute));

			var embedBuilder = new DiscordEmbedBuilder
			{
				Title = "Info",
				Url = sourceCodeLink,
				Description = desc,
				Timestamp = DateTimeOffset.Now,
				Color = DiscordColor.DarkBlue,
			}.WithThumbnail(context.Client.CurrentUser.AvatarUrl);
			embedBuilder.AddField("Links", links, true);
			embedBuilder.AddField("Versions", versions, true);

			await context.RespondAsync(embed: embedBuilder.Build());
		}

		[Command("DeleteMessages")]
		[Aliases("dmsg", "rm", "rmm")]
		[RequireOwner]
		public async Task DeleteMessagesCommand(CommandContext context, [RemainingText, Description("Messages Id's")]
												params ulong[] messages)
		{
			var msgsToDelete =
				(await context.Channel.GetMessagesBeforeAsync(context.Message.Id)).Where(x => x.Author.IsCurrent && messages.Contains(x.Id));
			foreach (var msg in msgsToDelete)
				await context.Channel.DeleteMessageAsync(msg);
		}

		[Command("Forcecheck")]
		[Aliases("fc")]
		[RequireOwner]
		[Hidden]
		public async Task ForceCheckCommand(CommandContext context, [RemainingText, Description("Update provider name")]
											string name)
		{
			name = name.Trim();
			BaseUpdateProvider? baseUpdateProvider;
			if (this._providersConfigurationService.Providers.TryGetValue(name, out var provider) &&
				provider is BaseUpdateProvider bup)
			{
				baseUpdateProvider = bup;
			}
			else
			{
				var upc = this._providersConfigurationService.Providers.Values.FirstOrDefault(p => p.Name.Where(char.IsUpper).ToString() == name);
				baseUpdateProvider = upc as BaseUpdateProvider;
			}

			if (baseUpdateProvider != null)
			{
				baseUpdateProvider.RestartTimer(TimeSpan.Zero);
				await context.RespondAsync(embed: EmbedTemplate.SuccessEmbed(context, "Success"));
			}
			else
			{
				await context.RespondAsync(embed: EmbedTemplate.ErrorEmbed(context, "Haven't found such update provider"));
			}


		}
	}
}