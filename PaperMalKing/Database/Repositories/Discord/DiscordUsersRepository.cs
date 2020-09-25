using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PaperMalKing.Database.Models;

namespace PaperMalKing.Database.Repositories.Discord
{
	public sealed class DiscordUsersRepository : BaseRepository
	{
		private readonly DbSet<DiscordUser> _users;
		public DiscordUsersRepository(DatabaseContext context) : base(context)
		{
			this._users = context.DiscordUsers;
		}

		public DiscordUser AddUser(long userId)
		{
			var user = new DiscordUser
			{
				DiscordUserId = userId,
				Guilds = new List<DiscordGuildUser>()
			};
			return this._users.Add(user).Entity;
		}

		public DiscordUser AddUser(DiscordUser user)
		{
			return this._users.Add(user).Entity;
		}

		public void RemoveUser(DiscordUser user)
		{
			this._users.Remove(user);
		}

		public async ValueTask AddUserToGuildAsync(long userId, long guildId)
		{
			var user = await this.GetUserByIdAsync(userId) ?? this.AddUser(userId);
			this.AddUserToGuild(user, guildId);
		}

		public DiscordUser? AddUserToGuild(DiscordUser user, long guildId)
		{
			var guildUser = new DiscordGuildUser
			{
				DiscordGuildId = guildId,
				DiscordUserId = user.DiscordUserId
			};
			user.Guilds.Add(guildUser);
			return this._users.Update(user)?.Entity;
		}

		public void RemoveUserFromGuild(DiscordUser user, long guildId)
		{
			var guildToRemove = user.Guilds.Find(guildUser => guildUser.DiscordGuildId == guildId);
			if (guildToRemove == null)
				return;
			user.Guilds.Remove(guildToRemove);
			this._users.Update(user);
		}

		public ConfiguredTaskAwaitable<DiscordUser> GetUserByIdAsync(long userId) =>
			this._users.FirstOrDefaultAsync(u => u.DiscordUserId == userId).ConfigureAwait(false)!;
	}
}