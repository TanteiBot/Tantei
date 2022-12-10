// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.Logging;
using PaperMalKing.Database.Models.Shikimori;
using PaperMalKing.UpdatesProviders.Base;
using PaperMalKing.UpdatesProviders.Base.Features;

namespace PaperMalKing.Shikimori.UpdateProvider;

[Group("shikimori")]
[Aliases("shiki")]
[Description("Commands for managing user updates from Shikimori.one")]
[ModuleLifespan(ModuleLifespan.Singleton)]
public sealed class ShikiCommands : BaseUpdateProviderUserCommandsModule<ShikiUserService>
{
	public ShikiCommands(ShikiUserService userService, ILogger<ShikiCommands> logger) : base(userService, logger)
	{
	}

	[Command("add")]
	[Description("Add your Shikimori account to being tracked")]
	public override Task AddUserCommand(CommandContext ctx, string username) => base.AddUserCommand(ctx, username);

	[Command("remove")]
	[Aliases("rm")]
	[Description("Remove your Shikimori account updates from being tracked")]
	public override Task RemoveUserInGuildCommand(CommandContext ctx) => base.RemoveUserInGuildCommand(ctx);

	[Command("list")]
	[Aliases("l")]
	[Description("List accounts of all tracked user's on Shikimori in this server")]
	public override Task ListUsersCommand(CommandContext ctx) => base.ListUsersCommand(ctx);

	[Command("removehere")]
	[Aliases("rmh")]
	[Description("Stop sending your updates to this server")]
	public override Task RemoveUserHereCommand(CommandContext ctx) => base.RemoveUserHereCommand(ctx);

#pragma warning disable CA1034
	[Group("features")]
	[Description("Manage your features for updates send from Shikimori.one")]
	[ModuleLifespan(ModuleLifespan.Singleton)]
	public sealed class ShikiUserFeaturesCommands : BaseUserFeaturesCommandsModule<ShikiUserFeatures>
	{
		public ShikiUserFeaturesCommands(IUserFeaturesService<ShikiUserFeatures> userFeaturesService, ILogger<ShikiUserFeaturesCommands> logger) :
			base(userFeaturesService, logger)
		{
		}

		[Command("enable")]
		[Description("Enable features for your updates")]
		public override Task EnableFeatureCommand(CommandContext context, [Description("Features to enable")] params ShikiUserFeatures[] features)
			=> base.EnableFeatureCommand(context, features);

		[Command("disable")]
		[Description("Disable features for your updates")]
		public override Task DisableFeatureCommand(CommandContext context,
												   [Description("Features to disable")] params ShikiUserFeatures[] features)
			=> base.DisableFeatureCommand(context, features);

		[Command("enabled")]
		[Description("Show features that are enabled for yourself")]
		public override Task EnabledFeaturesCommand(CommandContext context) => base.EnabledFeaturesCommand(context);

		[Command("list")]
		[Aliases("all")]
		[Description("Show all features that are available for updates from Shikimori.one")]
		public override Task ListFeaturesCommand(CommandContext context) => base.ListFeaturesCommand(context);
	}
}