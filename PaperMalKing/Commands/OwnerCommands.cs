using System;
using System.IO;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using PaperMalKing.Data;
using PaperMalKing.Services;
using Newtonsoft.Json;

namespace PaperMalKing.Commands
{
	[Group("owner")]
	[Aliases("o")]
	[Description("Commands for owner of the bot")]
	[RequireOwner]
	[ModuleLifespan(ModuleLifespan.Transient)]
	public sealed class OwnerCommands : BaseCommandModule
	{
		public DatabaseContext Database { get; set; }

		public BotConfig Config { get; set; }

		public MalService MalService { get; set; }

		public OwnerCommands(BotConfig config, DatabaseContext database, MalService malService)
		{
			this.Config = config;
			this.Database = database;
			this.MalService = malService;
		}

		[Command("UpdatePresence")]
		[Description("Updates bot presence")]
		[Aliases("updpr")]
		[Cooldown(5, 60, CooldownBucketType.Global)]
		public async Task UpdatePresence(CommandContext context,
		[Description("0 - Playing, 2 - Listening to, 3 - Watching")] int presenceType,
		[RemainingText, Description("Text that will be displayed in bots' status")] string presenceText)
		{
			if (string.IsNullOrWhiteSpace(presenceText))
				throw new ArgumentException("Presence text shouldn't be empty", nameof(presenceText));

			if (presenceText.Length > 128)
				throw new ArgumentException("Presence text length shouldn't be more than 128 characters.", nameof(presenceText));

			var actType = ActivityType.Watching;
			if (Enum.IsDefined(typeof(ActivityType), presenceText))
			{
				actType = (ActivityType) Enum.ToObject(typeof(ActivityType), presenceType);
				this.Config.Discord.ActivityType = presenceType;
			}

			this.Config.Discord.PresenceText = presenceText;

			var json = JsonConvert.SerializeObject(this.Config);
			await File.WriteAllTextAsync("testconfig.json", json);

			await context.Client.UpdateStatusAsync(new DiscordActivity(presenceText, actType), UserStatus.Online);
		}

	}
}
