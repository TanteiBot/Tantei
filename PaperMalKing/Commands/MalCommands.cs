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
		[Description("Add your account on MyAnimeList to list of tracked users. One discord user can add only one user on MyAnimeList")]
		public async Task Add(CommandContext context, [Description("Your username on MyAnimeList. Warning it's case sensitive!"), RemainingText] string username)
		{
			if (string.IsNullOrWhiteSpace(username))
				throw new ArgumentException("Username shouldn't be empty string", nameof(username));

			username = username.Trim();

			await this.MalService.AddUserAsync(context.User, username);

			var embed = EmbedTemplate.SuccessCommand(context.User,
				$"Succesfully added {username} in list of tracked users.");
			await context.RespondAsync(embed: embed.Build());
		}

		[Command("Remove")]
		[Description("Removes your account on MyAnimeList from being tracked.")]
		[Aliases("rm")]
		public async Task Remove(CommandContext context)
		{
			var userId = (long) context.User.Id;
			this.MalService.RemoveUser(userId);
			var embed = EmbedTemplate.SuccessCommand(context.User, "Successfully deleted you from tracked users");
			await context.RespondAsync(embed: embed.Build());
		}

		[Command("ForceRemove")]
		[Description("Removes other user's account from being tracked")]
		[Aliases("force_remove", "forcerm", "frm")]
		[OwnerOrPermission(Permissions.ManageGuild)]
		public async Task ForceRemove(CommandContext context, DiscordUser user)
		{
			if (user.IsBot)
				throw new ArgumentException("You can't delete bot from tracking",nameof(user));
			var userId = (long) user.Id;
			this.MalService.RemoveUser(userId);
			var embed = EmbedTemplate.SuccessCommand(context.User, $"Successfully deleted {user.Username} from tracked users");
			await context.RespondAsync(embed: embed.Build());
		}

		[Command("Update")]
		[Description("Updates your tracked username. Use this command if you changed your username on MyAnimeLits")]
		[Aliases("upd")]
		public async Task Update(CommandContext context, string newUsername)
		{
			if (string.IsNullOrWhiteSpace(newUsername))
				throw new ArgumentException("Username shouldn't be empty", nameof(newUsername));

			newUsername = newUsername.Trim();

			var userId =(long) context.User.Id;
			await this.MalService.UpdateAsync(userId, newUsername);
			var embed = EmbedTemplate.SuccessCommand(context.User, $"Successfully updated your username on MyAnimeList to {newUsername}");
			await context.RespondAsync(embed: embed.Build());
		}

		[Command("ForceCheck")]
		[Description("Checks MyAnimeListsUpdates right now.")]
		[Aliases("fc")]
		[OwnerOrPermission(Permissions.ManageGuild)]
		public async Task ForceCheck(CommandContext context)
		{
			if(!this.MalService.Updating)
				this.MalService.RestartTimer();
			var embed = EmbedTemplate.SuccessCommand(context.User, "Successfuly started checking task");
			await context.RespondAsync(embed: embed.Build());
		}

	}
}
