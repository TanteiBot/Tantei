using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PaperMalKing.Database.Models;
using PaperMalKing.Services;

namespace PaperMalKing.Database.Repositories.Discord
{
	public sealed class DiscordUsersRepository : BaseRepository
	{
		public DiscordUsersRepository(DatabaseContext context) : base(context)
		{ }

		public DiscordUser AddUser(long userId)
		{
			var user = new DiscordUser
			{
				DiscordUserId = userId,
				Guilds = new List<DiscordGuildUser>()
			};
			return this._context.DiscordUsers.Add(user).Entity;
		}

		public DiscordUser AddUser(DiscordUser user)
		{
			return this._context.DiscordUsers.Add(user).Entity;
		}

		public void RemoveUser(DiscordUser user)
		{
			this._context.DiscordUsers.Remove(user);
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
			return this._context.DiscordUsers.Update(user)?.Entity;
		}

		public void RemoveUserFromGuild(DiscordUser user, long guildId)
		{
			var guildToRemove = user.Guilds.Find(guildUser => guildUser.DiscordGuildId == guildId);
			if (guildToRemove == null)
				return;
			user.Guilds.Remove(guildToRemove);
			this._context.DiscordUsers.Update(user);
		}

		public Task<DiscordUser?> GetUserByIdAsync(long userId) =>
			this._context.DiscordUsers.FirstOrDefaultAsync(u => u.DiscordUserId == userId)!;

		public Task<int> SaveChangesAsync() => this._context.SaveChangesAsync();
	}
}