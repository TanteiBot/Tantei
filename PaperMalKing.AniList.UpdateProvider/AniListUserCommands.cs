using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.Logging;
using PaperMalKing.UpdatesProviders.Base;

namespace PaperMalKing.AniList.UpdateProvider
{
    [Group("AniList")]
    [Aliases("al")]
    public sealed class AniListUserCommands : BaseUpdateProviderUserCommandsModule
    {
        public AniListUserCommands(AniListUserService userService, ILogger<AniListUserCommands> logger) : base(userService,
            logger)
        {
        }

        [Command("add")]
        public override Task AddUserCommand(CommandContext ctx, string username) => base.AddUserCommand(ctx, username);

        [Command("remove")]
        [Aliases("rm")]
        public override Task RemoveUserInGuildCommand(CommandContext ctx) => base.RemoveUserInGuildCommand(ctx);

        [Command("list")]
        [Aliases("l")]
        public override Task ListUsersCommand(CommandContext ctx) => base.ListUsersCommand(ctx);

        [Command("removehere")]
        [Aliases("rmh")]
        public override Task RemoveUserHereCommand(CommandContext ctx) => base.RemoveUserHereCommand(ctx);
    }
}