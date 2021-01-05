using PaperMalKing.Database.Models;

namespace PaperMalKing.UpdatesProviders.Base
{
	public class BaseUser
	{
		public string Username { get; init; }

		public DiscordUser? DiscordUser { get; init; }

		public BaseUser(string username, DiscordUser discordUser = null)
		{
			this.Username = username;
			this.DiscordUser = discordUser;
		}
	}
}