using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.Logging;
using PaperMalKing.UpdatesProviders.Base;

namespace PaperMalKing.UpdatesProviders.MyAnimeList
{
	[Group(Constants.Name)]
	[Aliases("mal")]
	[Description("Commands for MyAnimeList update checker.")]
	[ModuleLifespan(ModuleLifespan.Singleton)]
	public class MalUserCommands : BaseUpdateProviderUserCommandsModule
	{
		/// <inheritdoc />
		public MalUserCommands(MalUserService userService, ILogger<BaseUpdateProviderUserCommandsModule> logger) : base(userService, logger)
		{ }

		[Command("add")]
		public override Task AddUserCommand(CommandContext ctx, string username) => base.AddUserCommand(ctx, username);

		[Command("remove")]
		[Aliases("rm")]
		public override Task RemoveUserInGuildCommand(CommandContext ctx) => base.RemoveUserInGuildCommand(ctx);

		[Command("list")]
		[Aliases("l")]
		public override Task ListUsersCommand(CommandContext ctx) => base.ListUsersCommand(ctx);
	}
}