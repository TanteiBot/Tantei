using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.Logging;
using PaperMalKing.UpdatesProviders.Base;

namespace PaperMalKing.AniList.UpdateProvider
{
    [Group("AniList")]
    [Aliases("al")]
    [Description("Commands for managing user updates from AniList.co")]
    [ModuleLifespan(ModuleLifespan.Singleton)]
    public sealed class AniListUserCommands : BaseUpdateProviderUserCommandsModule
    {
        public AniListUserCommands(AniListUserService userService, ILogger<AniListUserCommands> logger) : base(userService,
            logger)
        {
        }

        [Command("add")]
        [Description("Add your AniList account to being tracked")]
        public override Task AddUserCommand(CommandContext ctx, [Description("Your username on AniList")]
            string username) => base.AddUserCommand(ctx, username);

        [Command("remove")]
        [Aliases("rm")]
        [Description("Remove your AniList account updates from being tracked")]
        public override Task RemoveUserInGuildCommand(CommandContext ctx) => base.RemoveUserInGuildCommand(ctx);

        [Command("list")]
        [Aliases("l")]
        [Description("List accounts of all tracked users on AniList")]
        public override Task ListUsersCommand(CommandContext ctx) => base.ListUsersCommand(ctx);

        [Command("removehere")]
        [Aliases("rmh")]
        [Description("Stop sending your updates to this server")]
        public override Task RemoveUserHereCommand(CommandContext ctx) => base.RemoveUserHereCommand(ctx);
    }
}