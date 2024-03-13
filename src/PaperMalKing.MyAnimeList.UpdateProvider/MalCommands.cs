// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using DSharpPlus.SlashCommands.Attributes;
using Microsoft.Extensions.Logging;
using PaperMalKing.Database.Models.MyAnimeList;
using PaperMalKing.UpdatesProviders.Base;
using PaperMalKing.UpdatesProviders.Base.Features;

namespace PaperMalKing.MyAnimeList.UpdateProvider;

[SlashCommandGroup("mal", "Commands for interacting with MyAnimeList.net")]
[SlashModuleLifespan(SlashModuleLifespan.Singleton)]
[GuildOnly]
[SlashRequireGuild]
internal sealed class MalCommands : ApplicationCommandModule
{
	[SlashCommandGroup("user", "Commands for managing user updates from MyAnimeList.net")]
	[SlashModuleLifespan(SlashModuleLifespan.Singleton)]
	public sealed class MalUserCommands : BaseUpdateProviderUserCommandsModule<MalUserService, MalUser>
	{
		public MalUserCommands(MalUserService userService, ILogger<MalUserCommands> logger)
			: base(userService, logger)
		{
		}

		[SlashCommand("add", "Add your MyAnimeList account to being tracked")]
		public override Task AddUserCommand(InteractionContext context, [Option(nameof(username), "Your username on MyAnimeList.net")] string? username = null) =>
			base.AddUserCommand(context, username);

		[SlashCommand("remove", "Remove your MyAnimeList account updates from being tracked")]
		public override Task RemoveUserInGuildCommand(InteractionContext context) => base.RemoveUserInGuildCommand(context);

		[SlashCommand("list", "List accounts of all tracked user's on MyAnimeList in this server")]
		public override Task ListUsersCommand(InteractionContext context) => base.ListUsersCommand(context);

		[SlashCommand("removehere", "Stop sending your updates to this server")]
		public override Task RemoveUserHereCommand(InteractionContext context) => base.RemoveUserHereCommand(context);
	}

	[SlashCommandGroup("features", "Manage your features for updates send from MyAnimeList.net")]
	[SlashModuleLifespan(SlashModuleLifespan.Singleton)]
	public sealed class MalUserFeaturesCommands : BaseUserFeaturesCommandsModule<MalUser, MalUserFeatures>
	{
		public MalUserFeaturesCommands(BaseUserFeaturesService<MalUser, MalUserFeatures> userFeaturesService, ILogger<MalUserFeaturesCommands> logger)
			: base(userFeaturesService, logger)
		{
		}

		[SlashCommand("enable", "Enable features for your updates")]
		public override Task EnableFeatureCommand(
			InteractionContext context,
			[ChoiceProvider(typeof(FeaturesChoiceProvider<MalUserFeatures>)),
			Option("feature", "Feature to enable")]
			string unparsedFeature)
		{
			return base.EnableFeatureCommand(context, unparsedFeature);
		}

		[SlashCommand("disable", "Disable features for your updates")]
		public override Task DisableFeatureCommand(
			InteractionContext context,
			[ChoiceProvider(typeof(FeaturesChoiceProvider<MalUserFeatures>)),
			Option("feature", "Feature to enable")]
			string unparsedFeature) => base.DisableFeatureCommand(context, unparsedFeature);

		[SlashCommand("enabled", "Show features that are enabled for yourself")]
		public override Task EnabledFeaturesCommand(InteractionContext context) => base.EnabledFeaturesCommand(context);

		[SlashCommand("list", "Show all features that are available for updates from MyAnimeList.net")]
		public override Task ListFeaturesCommand(InteractionContext context) => base.ListFeaturesCommand(context);
	}
}