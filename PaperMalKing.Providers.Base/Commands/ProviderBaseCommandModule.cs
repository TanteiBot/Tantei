using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using PaperMalKing.Providers.Base.Attributes;

namespace PaperMalKing.Providers.Base.Commands
{
	public class ProviderBaseCommandModule<T> : BaseCommandModule where T : IProvider
	{
		protected readonly IUserManager UserManager;

		public ProviderBaseCommandModule(T provider)
		{
			this.UserManager = provider.UserManager;
		}

		[Command("add")]
		public virtual Task AddUser(CommandContext context, [Description("Your username on website")]
									string username)
		{
			if (string.IsNullOrWhiteSpace(username))
				throw new ArgumentException("You must provide valid username");
			return this.UserManager.AddUser(username, context.Member.Id);
		}

		[Command("remove")]
		[Description("Removes your account from tracked on this provider")]
		[Aliases("rm")]
		public virtual Task RemoveUser(CommandContext context)
		{
			return this.UserManager.RemoveUser(context.Member.Id);
		}

		[Command("forceremove")]
		[Aliases("frm")]
		[OwnerOrPermission(Permissions.ManageGuild)]
		public virtual Task AddUser(CommandContext context, DiscordMember member)
		{
			return this.UserManager.RemoveUser(member.Id);
		}
	}
}