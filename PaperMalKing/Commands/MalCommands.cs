using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using PaperMalKing.Attributes;
using PaperMalKing.Services;
using PaperMalKing.Utilities;


namespace PaperMalKing.Commands
{
	[Group("MyAnimeList")]
	[Description("Commands related to managing with MyAnimeList")]
	[Aliases("mal")]
	[ModuleLifespan(ModuleLifespan.Transient)]
	public sealed class MalCommands : BaseCommandModule
	{
		public MalService MalService { get; set; }

		public DatabaseContext Database { get; set; }

		public MalCommands(MalService malService, DatabaseContext database)
		{
			this.MalService = malService;
			this.Database = database;
		}

		[Command("Add")]
		[Description(
			"Add your account on MyAnimeList to list of tracked users. One discord user can add only one user on MyAnimeList")]
		public async Task AddCommand(CommandContext context,
			[Description("Your username on MyAnimeList. Warning it's case sensitive!"), RemainingText]
			string username)
		{
			if (string.IsNullOrWhiteSpace(username))
				throw new ArgumentException("Username shouldn't be empty string", nameof(username));

			username = username.Trim();

			await this.MalService.AddUserAsync(context.Member, username);

			var embed = EmbedTemplate.SuccessCommand(context.User,
				$"Successfully added {username} in list of tracked users.");
			await context.RespondAsync(embed: embed.Build());
		}

		[Command("AddHere")]
		[Description(
			"Adds your account on MyAnimeList to list of tracked users in this guild. You need to \"register\" your MAL account in other guild with username in it")]
		[Aliases("addh")]
		public async Task AddHereCommand(CommandContext context)
		{
			await this.MalService.AddUserHereAsync(context.Member);
			var embed = EmbedTemplate.SuccessCommand(context.User,
				"Successfully added you in list of tracked users in this guild");
			await context.RespondAsync(embed: embed.Build());
		}


		[Command("RemoveHere")]
		[Description("Removes your account on MyAnimeList from being tracked in this guild.")]
		[Aliases("rmh")]
		public async Task RemoveHereCommand(CommandContext context)
		{
			this.MalService.RemoveUserHere(context.Member);
			var embed = EmbedTemplate.SuccessCommand(context.User,
				"Successfully deleted you from tracked users in this guild.");
			await context.RespondAsync(embed: embed.Build());
		}

		[Command("RemoveEverywhere")]
		[Description(
			"Removes your account on MyAnimeList from being tracked in all guilds where you have been registered.")]
		[Aliases("rme")]
		public async Task RemoveEverywhereCommand(CommandContext context)
		{
			this.MalService.RemoveUserEverywhere(context.Member);
			var embed = EmbedTemplate.SuccessCommand(context.User,
				"Successfully deleted you from tracked users in all guilds where you have been registered.");
			await context.RespondAsync(embed: embed.Build());
		}

		[Command("ForceRemove")]
		[Description("Removes other user's account from being tracked")]
		[Aliases("force_remove", "forcerm", "frm")]
		[OwnerOrPermission(Permissions.ManageGuild)]
		public async Task ForceRemoveCommand(CommandContext context,
			[Description("Member to remove from tracked users in this guild")]
			DiscordMember member)
		{
			if (member.IsBot)
				throw new ArgumentException("You can't delete bot from tracking", nameof(member));
			this.MalService.RemoveUserHere(member);
			var embed = EmbedTemplate.SuccessCommand(context.User,
				$"Successfully deleted {member.Username} from tracked users");
			await context.RespondAsync(embed: embed.Build());
		}

		[Command("Update")]
		[Description("Updates your tracked username. Use this command if you changed your username on MyAnimeList")]
		[Aliases("upd")]
		public async Task UpdateCommand(CommandContext context, [Description("Your new username on MAL"), RemainingText]
			string newUsername)
		{
			if (string.IsNullOrWhiteSpace(newUsername))
				throw new ArgumentException("Username shouldn't be empty", nameof(newUsername));

			newUsername = newUsername.Trim();

			var userId = (long) context.User.Id;
			await this.MalService.UpdateUserAsync(userId, newUsername);
			var embed = EmbedTemplate.SuccessCommand(context.User,
				$"Successfully updated your username on MyAnimeList to {newUsername}");
			await context.RespondAsync(embed: embed.Build());
		}

		[Command("ForceCheck")]
		[Description("Checks MyAnimeListsUpdates right now.")]
		[Aliases("fc")]
		[RequireOwner]
		public async Task ForceCheckCommand(CommandContext context)
		{
			if (!this.MalService.Updating)
				this.MalService.RestartTimer();
			var embed = EmbedTemplate.SuccessCommand(context.User, "Successfully started checking task");
			await context.RespondAsync(embed: embed.Build());
		}

		[Command("ChannelAdd")]
		[Description("Adds channel to bot to which updates for users in this guild will be sent")]
		[Aliases("chnadd")]
		[OwnerOrPermission(Permissions.ManageGuild)]
		public async Task ChannelAddCommand(CommandContext context,
			[Description("Channel where all user-updates will be sent")]
			DiscordChannel channel)
		{
			var perms = channel.PermissionsFor(context.Guild.CurrentMember);
			if (!perms.HasPermission(Permissions.SendMessages) || !perms.HasPermission(Permissions.EmbedLinks))
				throw new Exception("Bot is lacking permissions to send messages and use embeds in this channel");
			var guildId = (long) context.Guild.Id;
			var channelId = (long) channel.Id;
			await this.MalService.AddChannelAsync(guildId, channelId);
			var embed = EmbedTemplate.SuccessCommand(context.User, "Successfully added channel");
			await context.RespondAsync(embed: embed.Build());
		}

		[Command("ChannelUpdate")]
		[Description("Updates a channel to which updates will be sent")]
		[Aliases("chnupd")]
		[OwnerOrPermission(Permissions.ManageGuild)]
		public async Task ChannelUpdateCommand(CommandContext context,
			[Description("New channel where user-updates will be sent")]
			DiscordChannel channel)
		{
			var perms = channel.PermissionsFor(context.Guild.CurrentMember);
			if (!perms.HasPermission(Permissions.SendMessages) || !perms.HasPermission(Permissions.EmbedLinks))
				throw new Exception("Bot is lacking permissions to send messages and use embeds in this channel");
			var guildId = (long) context.Guild.Id;
			var channelId = (long) channel.Id;
			await this.MalService.UpdateChannelAsync(guildId, channelId);
			var embed = EmbedTemplate.SuccessCommand(context.User, "Successfully updated channel");
			await context.RespondAsync(embed: embed.Build());
		}

		[Command("ChannelRemove")]
		[Description("Removes registered channel for this guild")]
		[Aliases("chnrm")]
		[OwnerOrPermission(Permissions.ManageGuild)]
		public async Task ChannelRemoveCommand(CommandContext context)
		{
			var guildId = (long) context.Guild.Id;
			this.MalService.RemoveChannel(guildId);
			var embed = EmbedTemplate.SuccessCommand(context.User, "Successfully removed channel");
			await context.RespondAsync(embed: embed.Build());
		}
	}
}