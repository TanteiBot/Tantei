// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaperMalKing.AniList.Wrapper.Abstractions;
using PaperMalKing.Common;
using PaperMalKing.Database;
using PaperMalKing.Database.Models;
using PaperMalKing.Database.Models.AniList;
using PaperMalKing.UpdatesProviders.Base;
using PaperMalKing.UpdatesProviders.Base.Exceptions;

namespace PaperMalKing.AniList.UpdateProvider;

internal sealed class AniListUserService : BaseUpdateProviderUserService<AniListUser>
{
	private readonly IAniListClient _client;
	private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
	public override string Name => ProviderConstants.NAME;

	public AniListUserService(ILogger<AniListUserService> logger, IAniListClient client, IDbContextFactory<DatabaseContext> dbContextFactory,
							  GeneralUserService userService) : base(logger, dbContextFactory, userService)
	{
		this._client = client;
		this._dbContextFactory = dbContextFactory;
	}

	public override async Task<BaseUser> AddUserAsync(ulong userId, ulong guildId, string? username = null)
	{
		using var db = this._dbContextFactory.CreateDbContext();
		var dbUser = db.AniListUsers.Include(su => su.DiscordUser).ThenInclude(du => du.Guilds).FirstOrDefault(su => su.DiscordUserId == userId);
		DiscordGuild? guild;
		if (dbUser is not null) // User already in db
		{
			if (dbUser.DiscordUser.Guilds.Any(g => g.DiscordGuildId == guildId))
			{
				throw new UserProcessingException(
					"You already have your account connected. If you want to switch to another account, remove current one, then add the new one.");
			}

			guild = db.DiscordGuilds.FirstOrDefault(g => g.DiscordGuildId == guildId);
			if (guild is null)
			{
				throw new UserProcessingException(BaseUser.FromUsername(username),
					"Current server is not in database, ask server administrator to add this server to bot");
			}

			dbUser.DiscordUser.Guilds.Add(guild);
			await db.SaveChangesAndThrowOnNoneAsync().ConfigureAwait(false);
			return BaseUser.FromUsername(username);
		}

		if (string.IsNullOrWhiteSpace(username))
		{
			throw new UserProcessingException(BaseUser.Empty, "You must provide username if you arent already tracked by this bot");
		}

		guild = db.DiscordGuilds.FirstOrDefault(g => g.DiscordGuildId == guildId);
		if (guild is null)
		{
			throw new UserProcessingException(BaseUser.FromUsername(username),
				"Current server is not in database, ask server administrator to add this server to bot");
		}

		var dUser = db.DiscordUsers.Include(x => x.Guilds).FirstOrDefault(du => du.DiscordUserId == userId);
		var response = await this._client.GetCompleteUserInitialInfoAsync(username).ConfigureAwait(false);
		var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		if (dUser is null)
		{
			dUser = new()
			{
				Guilds = new[] { guild },
				DiscordUserId = userId,
				BotUser = new()
			};
		}
		else if (dUser.Guilds.All(x => x.DiscordGuildId != guildId))
		{
			dUser.Guilds.Add(guild);
		}

		dbUser = new()
		{
			Favourites = response.Favourites.ConvertAll(f => new AniListFavourite
			{
				Id = f.Id,
				FavouriteType = (FavouriteType)f.Type
			}),
			Id = response.UserId!.Value,
			DiscordUser = dUser,
			LastActivityTimestamp = now,
			LastReviewTimestamp = now,
			FavouritesIdHash = Helpers.FavoritesHash(response.Favourites.Select(x => new FavoriteIdType(x.Id, (byte)x.Type)).ToArray())
		};
		dbUser.Favourites.ForEach(f =>
		{
			f.User = dbUser;
			f.UserId = dbUser.Id;
		});
		db.AniListUsers.Add(dbUser);
		await db.SaveChangesAndThrowOnNoneAsync().ConfigureAwait(false);
		return BaseUser.FromUsername(username);
	}

	public override IReadOnlyList<BaseUser> ListUsers(ulong guildId)
	{
		return this.ListUsersCore(guildId, u => u.LastActivityTimestamp, u => new BaseUser("", u.DiscordUser));
	}
}