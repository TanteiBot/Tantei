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
using PaperMalKing.AniList.Wrapper;
using PaperMalKing.Database;
using PaperMalKing.Database.Models;
using PaperMalKing.Database.Models.AniList;
using PaperMalKing.UpdatesProviders.Base;
using PaperMalKing.UpdatesProviders.Base.Exceptions;

namespace PaperMalKing.AniList.UpdateProvider
{
    public sealed class AniListUserService : IUpdateProviderUserService
    {
        private readonly AniListClient _client;
        private readonly IServiceProvider _serviceProvider;
        public string Name => Constants.NAME;

        public AniListUserService(AniListClient client, IServiceProvider serviceProvider)
        {
            this._client = client;
            this._serviceProvider = serviceProvider;
        }

        public async Task<BaseUser> AddUserAsync(string username, ulong userId, ulong guildId)
        {
			using var scope = this._serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            var dbUser = await db.AniListUsers.Include(su => su.DiscordUser).ThenInclude(du => du.Guilds)
                .FirstOrDefaultAsync(su => su.DiscordUserId == userId).ConfigureAwait(false);
            DiscordGuild guild;
            if (dbUser != null) // User already in db
			{
				if (dbUser.DiscordUser.Guilds.Any(g => g.DiscordGuildId == guildId))
					throw new UserProcessingException(
						"You already have your account connected. If you want to switch to another account, remove current one, then add the new one.");
                guild = await db.DiscordGuilds.FirstOrDefaultAsync(g => g.DiscordGuildId == guildId).ConfigureAwait(false);
                if (guild == null)
                    throw new UserProcessingException(new(username),
                        "Current server is not in database, ask server administrator to add this server to bot");

                dbUser.DiscordUser.Guilds.Add(guild);
                db.AniListUsers.Update(dbUser);
                await db.SaveChangesAndThrowOnNoneAsync().ConfigureAwait(false);
                return new(username);
            }

            guild = await db.DiscordGuilds.FirstOrDefaultAsync(g => g.DiscordGuildId == guildId).ConfigureAwait(false);
            if (guild == null)
                throw new UserProcessingException(new(username),
                    "Current server is not in database, ask server administrator to add this server to bot");
            var dUser = await db.DiscordUsers.FirstOrDefaultAsync(du => du.DiscordUserId == userId).ConfigureAwait(false);
            var response = await this._client.GetCompleteUserInitialInfoAsync(username).ConfigureAwait(false);
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            dbUser = new()
            {
                Favourites = response.Favourites.Select(f => new AniListFavourite {Id = f.Id, FavouriteType = (FavouriteType) f.Type}).ToList(),
                Id = response.UserId!.Value,
                DiscordUser = dUser,
                DiscordUserId = dUser.DiscordUserId,
                LastActivityTimestamp = now,
                LastReviewTimestamp = now
            };
            dbUser.Favourites.ForEach(f =>
            {
                f.User = dbUser;
                f.UserId = dbUser.Id;
            });
            await db.AniListUsers.AddAsync(dbUser).ConfigureAwait(false);
            await db.SaveChangesAndThrowOnNoneAsync().ConfigureAwait(false);
            return new(username);
        }

        public async Task<BaseUser> RemoveUserAsync(ulong userId)
        {
            using var scope = this._serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            var user = await db.AniListUsers.Include(su => su.DiscordUser).ThenInclude(du => du.Guilds)
                .FirstOrDefaultAsync(su => su.DiscordUserId == userId).ConfigureAwait(false);
            if (user == null)
                throw new UserProcessingException($"You weren't tracked by {this.Name} update checker");

            db.AniListUsers.Remove(user);
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

        public IAsyncEnumerable<BaseUser> ListUsersAsync(ulong guildId)
        {
            using var scope = this._serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            return db.AniListUsers.Include(su => su.DiscordUser).ThenInclude(du => du.Guilds).Select(su => new
                {
                    su.DiscordUser,
                    su.LastActivityTimestamp
                }).Where(u => u.DiscordUser.Guilds.Any(g => g.DiscordGuildId == guildId)).OrderByDescending(u => u.LastActivityTimestamp)
                .Select(u => new BaseUser("", u.DiscordUser)).AsNoTracking().AsAsyncEnumerable();
        }
    }
}