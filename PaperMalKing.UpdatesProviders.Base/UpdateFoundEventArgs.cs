using System;
using PaperMalKing.Database.Models;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;

namespace PaperMalKing.UpdatesProviders.Base
{
	public sealed class UpdateFoundEventArgs : EventArgs
	{
		public IUpdate Update { get; }

		public IUpdateProvider UpdateProvider { get; }

		public DiscordUser DiscordUser { get; }

		public UpdateFoundEventArgs(IUpdate update, IUpdateProvider updateProvider, DiscordUser discordUser)
		{
			this.Update = update;
			this.UpdateProvider = updateProvider;
			this.DiscordUser = discordUser;
		}
	}
}