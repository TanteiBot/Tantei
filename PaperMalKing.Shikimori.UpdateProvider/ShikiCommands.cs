// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System.Threading.Tasks;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using PaperMalKing.Database.Models.Shikimori;
using PaperMalKing.UpdatesProviders.Base;
using PaperMalKing.UpdatesProviders.Base.Features;

namespace PaperMalKing.Shikimori.UpdateProvider;

[SlashCommandGroup("shiki", "Commands for interacting with Shikimori.one")]
[SlashModuleLifespan(SlashModuleLifespan.Singleton)]
public sealed class ShikiCommands : ApplicationCommandModule
{
	[SlashCommandGroup("user", "Commands for managing user updates from Shikimori.one")]
	[SlashModuleLifespan(SlashModuleLifespan.Singleton)]
	public sealed class ShikiUserCommands : BaseUpdateProviderUserCommandsModule<ShikiUserService>
	{
		public ShikiUserCommands(ShikiUserService userService, ILogger<ShikiUserCommands> logger) : base(userService, logger)
		{ }

		[SlashCommand("add", "Add your Shikimori account to being tracked", true)]
		public override Task AddUserCommand(InteractionContext ctx, [Option("username", "Your username on Shikimori")] string username) =>
			base.AddUserCommand(ctx, username);

		[SlashCommand("remove", "Remove your Shikimori account updates from being tracked", true)]
		public override Task RemoveUserInGuildCommand(InteractionContext ctx) => base.RemoveUserInGuildCommand(ctx);

		[SlashCommand("list", "List accounts of all tracked user's on Shikimori in this server", true)]
		public override Task ListUsersCommand(InteractionContext ctx) => base.ListUsersCommand(ctx);

		[SlashCommand("removehere", "Stop sending your updates to this server", true)]
		public override Task RemoveUserHereCommand(InteractionContext ctx) => base.RemoveUserHereCommand(ctx);
	}

	[SlashCommandGroup("features", "Manage your features for updates send from Shikimori.one", true)]
	[SlashModuleLifespan(SlashModuleLifespan.Singleton)]
	public sealed class ShikiUserFeaturesCommands : BaseUserFeaturesCommandsModule<ShikiUserFeatures>
	{
		public ShikiUserFeaturesCommands(IUserFeaturesService<ShikiUserFeatures> userFeaturesService, ILogger<ShikiUserFeaturesCommands> logger) :
			base(userFeaturesService, logger)
		{ }

		[SlashCommand("enable", "Enable features for your updates", true)]
		public override Task EnableFeatureCommand(InteractionContext context,
												  [ChoiceProvider(typeof(FeaturesChoiceProvider<ShikiUserFeatures>)),
												   Option("feature", "Feature to enable")]
												  string unparsedFeature) => base.EnableFeatureCommand(context, unparsedFeature);

		[SlashCommand("disable", "Disable features for your updates", true)]
		public override Task DisableFeatureCommand(InteractionContext context,
												   [ChoiceProvider(typeof(FeaturesChoiceProvider<ShikiUserFeatures>)),
												    Option("feature", "Feature to disable")]
												   string unparsedFeature) => base.DisableFeatureCommand(context, unparsedFeature);

		[SlashCommand("enabled", "Show features that are enabled for yourself", true)]
		public override Task EnabledFeaturesCommand(InteractionContext context) => base.EnabledFeaturesCommand(context);

		[SlashCommand("list", "Show all features that are available for updates from Shikimori.one", true)]
		public override Task ListFeaturesCommand(InteractionContext context) => base.ListFeaturesCommand(context);
	}
}