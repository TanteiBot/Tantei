using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.Logging;
using PaperMalKing.UpdatesProviders.Base;

namespace PaperMalKing.UpdatesProviders.MyAnimeList
{
    [Group(Constants.Name)]
    [Aliases("mal")]
    [Description("Commands for managing user updates from MyAnimeList.net")]
    [ModuleLifespan(ModuleLifespan.Singleton)]
    public class MalUserCommands : BaseUpdateProviderUserCommandsModule
    {
        /// <inheritdoc />
        public MalUserCommands(MalUserService userService, ILogger<MalUserCommands> logger) : base(userService, logger)
        {
        }

        [Command("add")]
        [Description("Add your MyAnimeList account to being tracked")]
        public override Task AddUserCommand(CommandContext ctx, [Description("Your username on MyAnimeList")]
            string username) => base.AddUserCommand(ctx, username);

        [Command("remove")]
        [Aliases("rm")]
        [Description("Remove your MyAnimeList account updates from being tracked")]
        public override Task RemoveUserInGuildCommand(CommandContext ctx) => base.RemoveUserInGuildCommand(ctx);

        [Command("list")]
        [Aliases("l")]
        [Description("List accounts of all tracked user's on MyAnimeList")]
        public override Task ListUsersCommand(CommandContext ctx) => base.ListUsersCommand(ctx);

        [Command("removehere")]
        [Aliases("rmh")]
        [Description("Stop sending your updates to this server")]
        public override Task RemoveUserHereCommand(CommandContext ctx) => base.RemoveUserHereCommand(ctx);
    }
}