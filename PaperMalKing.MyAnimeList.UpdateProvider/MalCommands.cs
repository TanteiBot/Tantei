#region LICENSE

// PaperMalKing.
// Copyright (C) 2021-2022 N0D4N
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.Logging;
using PaperMalKing.Database.Models.MyAnimeList;
using PaperMalKing.UpdatesProviders.Base;
using PaperMalKing.UpdatesProviders.Base.Features;

namespace PaperMalKing.UpdatesProviders.MyAnimeList
{
	[Group(Constants.Name)]
	[Aliases("mal")]
	[Description("Commands for managing user updates from MyAnimeList.net")]
	[ModuleLifespan(ModuleLifespan.Singleton)]
	public sealed class MalCommands : BaseUpdateProviderUserCommandsModule<MalUserService>
	{
		/// <inheritdoc />
		public MalCommands(MalUserService userService, ILogger<MalCommands> logger) : base(userService, logger)
		{
		}

		[Command("add")]
		[Description("Add your MyAnimeList account to being tracked")]
		public override Task AddUserCommand(CommandContext ctx, [Description("Your username on MyAnimeList.net")]
											string username)
			=> base.AddUserCommand(ctx, username);

		[Command("remove")]
		[Aliases("rm")]
		[Description("Remove your MyAnimeList account updates from being tracked")]
		public override Task RemoveUserInGuildCommand(CommandContext ctx) => base.RemoveUserInGuildCommand(ctx);

		[Command("list")]
		[Aliases("l")]
		[Description("List accounts of all tracked user's on MyAnimeList in this server")]
		public override Task ListUsersCommand(CommandContext ctx) => base.ListUsersCommand(ctx);

		[Command("removehere")]
		[Aliases("rmh")]
		[Description("Stop sending your updates to this server")]
		public override Task RemoveUserHereCommand(CommandContext ctx) => base.RemoveUserHereCommand(ctx);

		#pragma warning disable CA1034
		[Group("features")]
		[Description("Manage your features for updates send from MyAnimeList.net")]
		[ModuleLifespan(ModuleLifespan.Singleton)]
		public sealed class MalUserFeaturesCommands : BaseUserFeaturesCommandsModule<MalUserFeatures>
		{
			public MalUserFeaturesCommands(IUserFeaturesService<MalUserFeatures> userFeaturesService, ILogger<MalUserFeaturesCommands> logger) :
				base(userFeaturesService, logger)
			{
			}

			[Command("enable")]
			[Description("Enable features for your updates")]
			public override Task EnableFeatureCommand(CommandContext context, [Description("Features to enable")] params MalUserFeatures[] features)
				=> base.EnableFeatureCommand(context, features);

			[Command("disable")]
			[Description("Disable features for your updates")]
			public override Task DisableFeatureCommand(CommandContext context, [Description("Features to disable")] params MalUserFeatures[] features)
				=> base.DisableFeatureCommand(context, features);

			[Command("enabled")]
			[Description("Show features that are enabled for yourself")]
			public override Task EnabledFeaturesCommand(CommandContext context) => base.EnabledFeaturesCommand(context);

			[Command("list")]
			[Aliases("all")]
			[Description("Show all features that are available for updates from MyAnimeList.net")]
			public override Task ListFeaturesCommand(CommandContext context) => base.ListFeaturesCommand(context);
		}
	}
}