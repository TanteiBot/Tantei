using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MoreLinq.Extensions;
using PaperMalKing.Database;
using PaperMalKing.Database.Models;
using PaperMalKing.Database.Models.Shikimori;
using PaperMalKing.Shikimori.Wrapper;
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
								 .FirstOrDefaultAsync(su => su.DiscordUserId == userId);
			DiscordGuild guild;
			if (dbUser != null) // User already in db
			{
				if (dbUser.DiscordUser.Guilds.Any(g => g.DiscordGuildId == guildId))
					return new(username);

				guild = await db.DiscordGuilds.FirstOrDefaultAsync(g => g.DiscordGuildId == guildId);
				if (guild == null)
					throw new UserProcessingException(new(username),
						"Current server is not in database, ask server administrator to add this server to bot");

				dbUser.DiscordUser.Guilds.Add(guild);
				db.ShikiUsers.Update(dbUser);
				await db.SaveChangesAndThrowOnNoneAsync();
				return new(username);
			}

			guild = await db.DiscordGuilds.FirstOrDefaultAsync(g => g.DiscordGuildId == guildId);
			if (guild == null)
				throw new UserProcessingException(new(username),
					"Current server is not in database, ask server administrator to add this server to bot");
			var dUser = await db.DiscordUsers.FirstOrDefaultAsync(du => du.DiscordUserId == userId);
			var shikiUser = await this._client.GetUserAsync(username);
			var history = await this._client.GetUserHistoryAsync(shikiUser.Id, 1, 1);
			var favourites = await this._client.GetUserFavouritesAsync(shikiUser.Id);
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
				DiscordUser = dUser ?? new()
				{
					BotUser = new(),
					Guilds = new DiscordGuild[1] {guild},
					DiscordUserId = userId,
				},
				DiscordUserId = userId,
				LastHistoryEntryId = history.Data.MaxBy(he => he.Id).First().Id
			};
			dbUser.Favourites.ForEach(f => f.User = dbUser);
			await db.ShikiUsers.AddAsync(dbUser);
			await db.SaveChangesAndThrowOnNoneAsync();
			return new(username);
		}

		/// <inheritdoc />
		public async Task<BaseUser> RemoveUserAsync(ulong userId)
		{
			using var scope = this._serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
			var user = await db.ShikiUsers.Include(su => su.DiscordUser).ThenInclude(du => du.Guilds)
							   .FirstOrDefaultAsync(su => su.DiscordUserId == userId);
			if (user == null)
				throw new UserProcessingException($"You weren't tracked by {this.Name} update checker");

			db.ShikiUsers.Remove(user);
			await db.SaveChangesAndThrowOnNoneAsync();
			return new("");
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