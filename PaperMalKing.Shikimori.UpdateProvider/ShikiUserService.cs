#region LICENSE
// PaperMalKing.
// Copyright (C) 2021 N0D4N
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PaperMalKing.Database;
using PaperMalKing.Database.Models;
using PaperMalKing.Database.Models.Shikimori;
using PaperMalKing.Shikimori.Wrapper;
using PaperMalKing.Shikimori.Wrapper.Models;
using PaperMalKing.UpdatesProviders.Base;
using PaperMalKing.UpdatesProviders.Base.Exceptions;

namespace PaperMalKing.Shikimori.UpdateProvider
{
	public sealed class ShikiUserService : IUpdateProviderUserService
	{
		private readonly ShikiClient _client;
		private readonly ILogger<ShikiUserService> _logger;
		private readonly IServiceProvider _serviceProvider;

		/// <inheritdoc />
		public string Name => Constants.NAME;

		public ShikiUserService(ShikiClient client, ILogger<ShikiUserService> logger, IServiceProvider serviceProvider)
		{
			this._client = client;
			this._logger = logger;
			this._serviceProvider = serviceProvider;
		}

		/// <inheritdoc />
		public async Task<BaseUser> AddUserAsync(string username, ulong userId, ulong guildId)
		{
			using var scope = this._serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
			var dbUser = await db.ShikiUsers.Include(su => su.DiscordUser).ThenInclude(du => du.Guilds)
								 .FirstOrDefaultAsync(su => su.DiscordUserId == userId).ConfigureAwait(false);
			DiscordGuild guild;
			if (dbUser != null) // User already in db
			{
				if (dbUser.DiscordUser.Guilds.Any(g => g.DiscordGuildId == guildId))
					return new(username);

				guild = await db.DiscordGuilds.FirstOrDefaultAsync(g => g.DiscordGuildId == guildId).ConfigureAwait(false);
				if (guild == null)
					throw new UserProcessingException(new(username),
						"Current server is not in database, ask server administrator to add this server to bot");

				dbUser.DiscordUser.Guilds.Add(guild);
				db.ShikiUsers.Update(dbUser);
				await db.SaveChangesAndThrowOnNoneAsync().ConfigureAwait(false);
				return new(username);
			}

			guild = await db.DiscordGuilds.FirstOrDefaultAsync(g => g.DiscordGuildId == guildId).ConfigureAwait(false);
			if (guild == null)
				throw new UserProcessingException(new(username),
					"Current server is not in database, ask server administrator to add this server to bot");
			var dUser = await db.DiscordUsers.FirstOrDefaultAsync(du => du.DiscordUserId == userId).ConfigureAwait(false);
			var shikiUser = await this._client.GetUserAsync(username).ConfigureAwait(false);
			var history = await this._client.GetUserHistoryAsync(shikiUser.Id, 1, 1, HistoryRequestOptions.Any).ConfigureAwait(false);
			var favourites = await this._client.GetUserFavouritesAsync(shikiUser.Id).ConfigureAwait(false);
			dbUser = new()
			{
				Favourites = favourites.AllFavourites.Select(f => new ShikiFavourite()
				{
					Id = f.Id,
					Name = f.Name,
					FavType = f.GenericType!
					// User = dbUser
				}).ToList(),
				Id = shikiUser.Id,
				Features = ShikiUserFeatures.None.GetDefault(),
				DiscordUser = dUser ?? new()
				{
					BotUser = new(),
					Guilds = new DiscordGuild[1] {guild},
					DiscordUserId = userId,
				},
				DiscordUserId = userId,
				LastHistoryEntryId = history.Data.Max(he => he.Id)
			};
			dbUser.Favourites.ForEach(f => f.User = dbUser);
			await db.ShikiUsers.AddAsync(dbUser).ConfigureAwait(false);
			await db.SaveChangesAndThrowOnNoneAsync().ConfigureAwait(false);
			return new(username);
		}

		/// <inheritdoc />
		public async Task<BaseUser> RemoveUserAsync(ulong userId)
		{
			using var scope = this._serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
			var user = await db.ShikiUsers.Include(su => su.DiscordUser).ThenInclude(du => du.Guilds)
							   .FirstOrDefaultAsync(su => su.DiscordUserId == userId).ConfigureAwait(false);
			if (user == null)
				throw new UserProcessingException($"You weren't tracked by {this.Name} update checker");

			db.ShikiUsers.Remove(user);
			await db.SaveChangesAndThrowOnNoneAsync().ConfigureAwait(false);
			return new("");
		}

		public async Task RemoveUserHereAsync(ulong userId, ulong guildId)
		{
			using var scope = this._serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
			var user = await db.DiscordUsers.Include(du => du.Guilds).FirstOrDefaultAsync(du => du.DiscordUserId == userId).ConfigureAwait(false);
			if (user == null)
				throw new UserProcessingException("You weren't registered in bot");
			var guild = user.Guilds.FirstOrDefault(g => g.DiscordGuildId == guildId);
			if (guild == null)
				throw new UserProcessingException("You weren't registered in this server");

			user.Guilds.Remove(guild);
			db.DiscordUsers.Update(user);
			await db.SaveChangesAndThrowOnNoneAsync().ConfigureAwait(false);
		}

		/// <inheritdoc />
		public IAsyncEnumerable<BaseUser> ListUsersAsync(ulong guildId)
		{
			using var scope = this._serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
			return db.ShikiUsers.Include(su => su.DiscordUser).ThenInclude(du => du.Guilds).Select(su => new
					 {
						 su.DiscordUser,
						 su.LastHistoryEntryId
					 }).Where(u => u.DiscordUser.Guilds.Any(g => g.DiscordGuildId == guildId)).OrderByDescending(u => u.LastHistoryEntryId)
					 .Select(u => new BaseUser("", u.DiscordUser)).AsNoTracking().AsAsyncEnumerable();
		}
	}
}