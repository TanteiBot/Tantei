using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Microsoft.Extensions.Logging;
using PaperMalKing.UpdatesProviders.Base;

namespace PaperMalKing.Shikimori.UpdateProvider
{
	[Group("shikimori")]
	[Aliases("shiki")]
	public sealed class ShikiUserCommands : BaseUpdateProviderUserCommandsModule
	{
		private readonly ShikiUserService _userService;

		/// <inheritdoc />
		public ShikiUserCommands(ShikiUserService userService, ILogger<BaseUpdateProviderUserCommandsModule> logger) : base(userService, logger)
		{
			this._userService = userService;
		}

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