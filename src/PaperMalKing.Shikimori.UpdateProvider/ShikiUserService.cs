// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
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

internal sealed class ShikiUserService(IShikiClient _client, ILogger<ShikiUserService> logger, IDbContextFactory<DatabaseContext> dbContextFactory, GeneralUserService userService)
	: BaseUpdateProviderUserService<ShikiUser>(logger, dbContextFactory, userService)
{
	public override string Name => Constants.Name;

	[SuppressMessage("Roslynator", "RCS1261:Resource can be disposed asynchronously", Justification = "Sqlite does not support async")]
	public override async Task<BaseUser> AddUserAsync(ulong userId, ulong guildId, string? username = null)
	{
		using var db = this.DbContextFactory.CreateDbContext();
		var dbUser = db.ShikiUsers.TagWith("Query user when trying to add one").TagWithCallSite().Include(su => su.DiscordUser).ThenInclude(du => du.Guilds).FirstOrDefault(su => su.DiscordUserId == userId);
		DiscordGuild? guild;
		if (dbUser != null)
		{
			// User already in db
			if (dbUser.DiscordUser.Guilds.Any(g => g.DiscordGuildId == guildId))
			{
				throw new UserProcessingException(
					"You already have your account connected. If you want to switch to another account, remove current one, then add the new one.");
			}

			guild = db.GetGuildById(guildId);
			if (guild is null)
			{
				throw new UserProcessingException(BaseUser.FromUsername(username),
					"Current server is not in database, ask server administrator to add this server to bot");
			}

			dbUser.DiscordUser.Guilds.Add(guild);
			await db.SaveChangesAndThrowOnNoneAsync();
			return BaseUser.FromUsername(username);
		}

		guild = db.GetGuildById(guildId);
		if (guild is null)
		{
			throw new UserProcessingException(BaseUser.FromUsername(username),
				"Current server is not in database, ask server administrator to add this server to bot");
		}

		if (string.IsNullOrWhiteSpace(username))
		{
			throw new UserProcessingException(BaseUser.Empty, "You must provide username if you arent already tracked by this bot");
		}

		var dUser = db.GetDiscordUserById(userId);
		var shikiUser = await _client.GetUserByNicknameAsync(username, CancellationToken.None);
		var history = await _client.GetUserHistoryAsync(shikiUser.Id, 1, 1, HistoryRequestOptions.Any, CancellationToken.None);

		if (history.Data is [])
		{
			throw new UserProcessingException(BaseUser.Empty,
				$"Your history is unaccessible, make sure `{Wrapper.Abstractions.Constants.BaseUrl}/{username}/history` can be accessed from incognito/private window in your browser");
		}

		var favourites = await _client.GetUserFavouritesAsync(shikiUser.Id, CancellationToken.None);
		var achievements = await _client.GetUserAchievementsAsync(shikiUser.Id, CancellationToken.None);
		if (dUser is null)
		{
			dUser = new()
			{
				BotUser = new(),
				Guilds = [guild],
				DiscordUserId = userId,
			};
		}
		else if (dUser.Guilds.All(x => x.DiscordGuildId != guildId))
		{
			dUser.Guilds.Add(guild);
		}
		else
		{
			// User is already in the guild, handled above
		}

		dbUser = new()
		{
			Favourites = [.. favourites.AllFavourites.Select(f => new ShikiFavourite
			{
				Id = f.Id,
				Name = f.Name,
				FavType = f.GenericType!,

				// User = dbUser
			}),],
			Id = shikiUser.Id,
			Features = ShikiUserFeatures.Default,
			DiscordUser = dUser,
			DiscordUserId = userId,
			LastHistoryEntryId = history.Data.Max(he => he.Id),
			FavouritesIdHash = HashHelpers.FavoritesHash(favourites.AllFavourites.ToFavoriteIdType()),
			Achievements = [.. achievements.Select(x => new ShikiDbAchievement
			{
				NekoId = x.Id,
				Level = x.Level,
			}),],
			Colors = [],
		};
		dbUser.Favourites.ForEach(f => f.User = dbUser);
		db.ShikiUsers.Add(dbUser);
		await db.SaveChangesAndThrowOnNoneAsync();
		return BaseUser.FromUsername(username);
	}

	public override IReadOnlyList<BaseUser> ListUsers(ulong guildId)
	{
		return this.ListUsersCore(guildId, static u => u.LastHistoryEntryId, static u => new("", u.DiscordUser));
	}
}