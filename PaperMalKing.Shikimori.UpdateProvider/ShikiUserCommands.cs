#region LICENSE
// PaperMalKing.
// Copyright (C) 2021 N0D4N
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
using PaperMalKing.UpdatesProviders.Base;

namespace PaperMalKing.Shikimori.UpdateProvider
{
    [Group("shikimori")]
    [Aliases("shiki")]
    [Description("Commands for managing user updates from Shikimori.one")]
    [ModuleLifespan(ModuleLifespan.Singleton)]
    public sealed class ShikiUserCommands : BaseUpdateProviderUserCommandsModule
    {
        /// <inheritdoc />
        public ShikiUserCommands(ShikiUserService userService, ILogger<ShikiUserCommands> logger) : base(userService, logger)
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
    }
}