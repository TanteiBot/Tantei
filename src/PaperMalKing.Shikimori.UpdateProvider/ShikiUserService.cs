// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaperMalKing.Common;
using PaperMalKing.Database;
using PaperMalKing.Database.Models;
using PaperMalKing.Database.Models.Shikimori;
using PaperMalKing.Shikimori.Wrapper.Abstractions;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models;
using PaperMalKing.UpdatesProviders.Base;
using PaperMalKing.UpdatesProviders.Base.Exceptions;

namespace PaperMalKing.Shikimori.UpdateProvider;

internal sealed class ShikiUserService : BaseUpdateProviderUserService<ShikiUser>
{
	private readonly IShikiClient _client;

	public override string Name => Constants.NAME;

	public ShikiUserService(IShikiClient client, ILogger<ShikiUserService> logger, IDbContextFactory<DatabaseContext> dbContextFactory, GeneralUserService userService) : base(logger, dbContextFactory, userService)
	{
		this._client = client;
	}

	public override async Task<BaseUser> AddUserAsync(ulong userId, ulong guildId, string? username = null)
	{
		using var db = this.DbContextFactory.CreateDbContext();
		var dbUser = db.ShikiUsers.TagWith("Query user when trying to add one").TagWithCallSite().Include(su => su.DiscordUser).ThenInclude(du => du.Guilds).FirstOrDefault(su => su.DiscordUserId == userId);
		DiscordGuild? guild;
		if (dbUser != null) // User already in db
		{
			if (dbUser.DiscordUser.Guilds.Any(g => g.DiscordGuildId == guildId))
			{
				throw new UserProcessingException(
					"You already have your account connected. If you want to switch to another account, remove current one, then add the new one.");
			}

			guild = db.DiscordGuilds.TagWith("Query guild to add existing user to it").TagWithCallSite().FirstOrDefault(g => g.DiscordGuildId == guildId);
			if (guild is null)
			{
				throw new UserProcessingException(BaseUser.FromUsername(username),
					"Current server is not in database, ask server administrator to add this server to bot");
			}

			dbUser.DiscordUser.Guilds.Add(guild);
			await db.SaveChangesAndThrowOnNoneAsync().ConfigureAwait(false);
			return BaseUser.FromUsername(username);
		}

		guild = db.DiscordGuilds.TagWith("Query guild to add new user to it").TagWithCallSite().FirstOrDefault(g => g.DiscordGuildId == guildId);
		if (guild is null)
		{
			throw new UserProcessingException(BaseUser.FromUsername(username),
				"Current server is not in database, ask server administrator to add this server to bot");
		}

		if (string.IsNullOrWhiteSpace(username))
		{
			throw new UserProcessingException(BaseUser.Empty, "You must provide username if you arent already tracked by this bot");
		}
		var dUser = db.DiscordUsers.TagWith("Query discord user to link AniList user to it").TagWithCallSite().Include(x => x.Guilds).FirstOrDefault(du => du.DiscordUserId == userId);
		var shikiUser = await this._client.GetUserAsync(username).ConfigureAwait(false);
		var history = await this._client.GetUserHistoryAsync(shikiUser.Id, 1, 1, HistoryRequestOptions.Any).ConfigureAwait(false);
		var favourites = await this._client.GetUserFavouritesAsync(shikiUser.Id).ConfigureAwait(false);
		var achievements = await this._client.GetUserAchievementsAsync(shikiUser.Id).ConfigureAwait(false);
		if (dUser is null)
		{
			dUser = new()
			{
				BotUser = new(),
				Guilds = new[] { guild },
				DiscordUserId = userId,
			};
		}
		else if (dUser.Guilds.All(x => x.DiscordGuildId != guildId))
		{
			dUser.Guilds.Add(guild);
		}
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
			DiscordUser = dUser,
			DiscordUserId = userId,
			LastHistoryEntryId = history.Data.Max(he => he.Id),
			FavouritesIdHash = Helpers.FavoritesHash(favourites.AllFavourites.Select(x=> new FavoriteIdType(x.Id, (byte)x.GenericType![0])).ToArray()),
			Achievements = achievements.Select(x=> new  ShikiDbAchievement
			{
				NekoId = x.Id,
				Level = x.Level
			}).ToList()
		};
		dbUser.Favourites.ForEach(f => f.User = dbUser);
		db.ShikiUsers.Add(dbUser);
		await db.SaveChangesAndThrowOnNoneAsync().ConfigureAwait(false);
		return BaseUser.FromUsername(username);
	}

	public override IReadOnlyList<BaseUser> ListUsers(ulong guildId)
	{
		return this.ListUsersCore(guildId, u => u.LastHistoryEntryId, u => new BaseUser("", u.DiscordUser));
	}
}