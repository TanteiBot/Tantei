using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PaperMalKing.Database;
using PaperMalKing.Database.Models;
using PaperMalKing.Database.Models.MyAnimeList;
using PaperMalKing.MyAnimeList.Wrapper;
using PaperMalKing.UpdatesProviders.Base;
using PaperMalKing.UpdatesProviders.Base.Exceptions;

namespace PaperMalKing.UpdatesProviders.MyAnimeList
{
	public sealed class MalUserService : IUpdateProviderUserService
	{
		private readonly MyAnimeListClient _client;

		private readonly ILogger<MalUserService> _logger;
		private readonly IServiceProvider _serviceProvider;


		public MalUserService(MyAnimeListClient client, ILogger<MalUserService> logger, IServiceProvider serviceProvider)
		{
			this._client = client;
			this._logger = logger;
			this._serviceProvider = serviceProvider;
		}

		/// <inheritdoc />
		public string Name => Constants.Name;

		public async Task<BaseUser> AddUserAsync(string username, ulong userId, ulong guildId)
		{
			using var scope = this._serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
			var dbUser = await db.MalUsers.Include(u => u.DiscordUser).ThenInclude(du => du.Guilds)
								 .FirstOrDefaultAsync(u => u.DiscordUser.DiscordUserId == userId);
			DiscordGuild guild;
			if (dbUser != null) // User already in DB
			{
				if (dbUser.DiscordUser.Guilds.Any(g => g.DiscordGuildId == guildId)) // User already in specified guild
				{
					return new(dbUser.Username);
				}

				guild = await db.DiscordGuilds.FirstOrDefaultAsync(g => g.DiscordGuildId == guildId);
				if (guild == null)
					throw new UserProcessingException(new(username),
						"Current server is not in database, ask server administrator to add this server to bot");
				dbUser.DiscordUser.Guilds.Add(guild);
				db.MalUsers.Update(dbUser);
				await db.SaveChangesAndThrowOnNoneAsync();
				return new(dbUser.Username);
			}

			guild = await db.DiscordGuilds.FirstOrDefaultAsync(g => g.DiscordGuildId == guildId);
			if (guild == null)
				throw new UserProcessingException(new(username),
					"Current server is not in database, ask server administrator to add this server to bot");

			var duser = await db.DiscordUsers.FirstOrDefaultAsync(user => user.DiscordUserId == userId);
			var mUser = await this._client.GetUserAsync(username);
			var now = DateTimeOffset.Now;
			dbUser = new()
			{
				UserId = mUser.Id,
				Username = mUser.Username,
				DiscordUser = duser ?? new()
				{
					Guilds = new DiscordGuild[1] {guild},
					DiscordUserId = userId,
					BotUser = new()
				},
				LastUpdatedAnimeListTimestamp = now,
				LastUpdatedMangaListTimestamp = now,
				LastAnimeUpdateHash = mUser.LatestAnimeUpdate?.Hash.ToHashString() ?? "",
				LastMangaUpdateHash = mUser.LatestMangaUpdate?.Hash.ToHashString() ?? ""
			};
			dbUser.FavoriteAnimes = mUser.Favorites.FavoriteAnime.Select(anime => anime.ToMalFavoriteAnime(dbUser)).ToList();
			dbUser.FavoriteMangas = mUser.Favorites.FavoriteManga.Select(manga => manga.ToMalFavoriteManga(dbUser)).ToList();
			dbUser.FavoriteCharacters = mUser.Favorites.FavoriteCharacters.Select(character => character.ToMalFavoriteCharacter(dbUser)).ToList();
			dbUser.FavoritePeople = mUser.Favorites.FavoritePeople.Select(person => person.ToMalFavoritePerson(dbUser)).ToList();
			await db.MalUsers.AddAsync(dbUser);
			await db.SaveChangesAndThrowOnNoneAsync();
			return new(dbUser.Username);
		}

		/// <inheritdoc />
		public async Task<BaseUser> RemoveUserAsync(ulong userId)
		{
			using var scope = this._serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
			var user = await db.MalUsers.Include(malUser => malUser.DiscordUser).ThenInclude(du => du.Guilds).Select(u => new MalUser
			{
				DiscordUser = u.DiscordUser,
				Username = u.Username,
				UserId = u.UserId
			}).FirstOrDefaultAsync(u => u.DiscordUser.DiscordUserId == userId);
			if (user == null)
			{
				throw new UserProcessingException($"You weren't tracked by {this.Name} update checker");
			}

			db.MalUsers.Remove(user);
			await db.SaveChangesAndThrowOnNoneAsync();
			return new(user.Username);
		}

		public async Task RemoveUserHereAsync(ulong userId, ulong guildId)
		{
			using var scope = this._serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
			var user = await db.DiscordUsers.Include(du => du.Guilds).FirstOrDefaultAsync(du => du.DiscordUserId == userId);
			if (user == null)
				throw new UserProcessingException("You weren't registered in bot");
			var guild = user.Guilds.FirstOrDefault(g => g.DiscordGuildId == guildId);
			if (guild == null)
				throw new UserProcessingException("You weren't registered in this server");

			user.Guilds.Remove(guild);
			db.DiscordUsers.Update(user);
			await db.SaveChangesAndThrowOnNoneAsync();
		}

		/// <inheritdoc />
		public IAsyncEnumerable<BaseUser> ListUsersAsync(ulong guildId)
		{
			using var scope = this._serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
			return db.MalUsers.Include(malUser => malUser.DiscordUser).ThenInclude(du => du.Guilds).Select(u => new MalUser
					 {
						 DiscordUser = u.DiscordUser,
						 Username = u.Username,
						 LastUpdatedAnimeListTimestamp = u.LastUpdatedAnimeListTimestamp
					 }).Where(u => u.DiscordUser.Guilds.Any(g => g.DiscordGuildId == guildId)).OrderByDescending(u => u.LastUpdatedAnimeListTimestamp)
					 .Select(mu => new BaseUser(mu.Username, mu.DiscordUser)).AsNoTracking().AsAsyncEnumerable();
		}
	}
}