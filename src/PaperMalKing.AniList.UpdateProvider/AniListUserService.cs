// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2025 N0D4N

using System.Diagnostics.CodeAnalysis;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaperMalKing.AniList.Wrapper.Abstractions;
using PaperMalKing.AniList.Wrapper.Abstractions.Models;
using PaperMalKing.AniList.Wrapper.Abstractions.Models.Enums;
using PaperMalKing.Common;
using PaperMalKing.Database;
using PaperMalKing.Database.Models.AniList;
using PaperMalKing.UpdatesProviders.Base;
using PaperMalKing.UpdatesProviders.Base.Exceptions;
using DiscordGuild = PaperMalKing.Database.Models.DiscordGuild;
using FavouriteType = PaperMalKing.Database.Models.AniList.FavouriteType;

namespace PaperMalKing.AniList.UpdateProvider;

internal sealed class AniListUserService(ILogger<AniListUserService> logger, IAniListClient _client, IDbContextFactory<DatabaseContext> dbContextFactory, GeneralUserService userService)
	: BaseUpdateProviderUserService<AniListUser>(logger, dbContextFactory, userService)
{
	public override string Name => ProviderConstants.Name;

	[SuppressMessage("Roslynator", "RCS1261:Resource can be disposed asynchronously", Justification = "Sqlite does not support async")]
	public override async Task<BaseUser> AddUserAsync(ulong userId, ulong guildId, string? username = null)
	{
		using var db = this.DbContextFactory.CreateDbContext();
		var dbUser = db.AniListUsers.TagWith("Query user when trying to add one").TagWithCallSite().Include(su => su.DiscordUser)
					   .ThenInclude(du => du.Guilds).FirstOrDefault(su => su.DiscordUserId == userId);
		DiscordGuild? guild;
		if (dbUser is not null)
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
				throw new UserProcessingException(BaseUser.FromUsername(username), "Current server is not in database, ask server administrator to add this server to bot");
			}

			dbUser.DiscordUser.Guilds.Add(guild);
			await db.SaveChangesAndThrowOnNoneAsync();
			return BaseUser.FromUsername(username);
		}

		if (string.IsNullOrWhiteSpace(username))
		{
			throw new UserProcessingException(BaseUser.Empty, "You must provide username if you arent already tracked by this bot");
		}

		guild = db.GetGuildById(guildId);
		if (guild is null)
		{
			throw new UserProcessingException(BaseUser.FromUsername(username), "Current server is not in database, ask server administrator to add this server to bot");
		}

		var dUser = db.GetDiscordUserById(userId);
		var response = await _client.GetCompleteUserInitialInfoAsync(username, CancellationToken.None);
		var now = TimeProvider.System.GetUtcNow().ToUnixTimeSeconds();
		if (dUser is null)
		{
			dUser = new()
			{
				Guilds = [guild],
				DiscordUserId = userId,
				BotUser = new(),
			};
		}
		else if (dUser.Guilds.All(x => x.DiscordGuildId != guildId))
		{
			dUser.Guilds.Add(guild);
		}
		else
		{
			// Case is handled above
		}

		dbUser = new()
		{
			Favourites = response.Favourites.ConvertAll(f => new AniListFavourite
			{
				Id = f.Id,
				FavouriteType = (FavouriteType)f.Type,
			}),
			Id = response.UserId!.Value,
			DiscordUser = dUser,
			LastActivityTimestamp = now,
			LastReviewTimestamp = now,
			FavouritesIdHash = HashHelpers.FavoritesHash(response.Favourites.ToFavoriteIdType()),
			Features = AniListUserFeatures.Default,
			Colors = [],
		};
		dbUser.Favourites.ForEach(f =>
		{
			f.User = dbUser;
			f.UserId = dbUser.Id;
		});
		db.AniListUsers.Add(dbUser);
		await db.SaveChangesAndThrowOnNoneAsync();
		return BaseUser.FromUsername(username);
	}

	public override IReadOnlyList<BaseUser> ListUsers(ulong guildId)
	{
		return this.ListUsersCore(guildId, static u => u.LastActivityTimestamp, static u => new("", u.DiscordUser));
	}

	[SuppressMessage("Roslynator", "RCS1261:Resource can be disposed asynchronously", Justification = "Sqlite does not support async")]
	public async Task<DiscordEmbed?> SearchMediaAsync(ulong userId, string query, ListType type)
	{
		using var db = this.DbContextFactory.CreateDbContext();
		var dbUser = db.AniListUsers.TagWith("Query user when searching for media").TagWithCallSite().FirstOrDefault(su => su.DiscordUserId == userId);

		var features = dbUser?.Features ?? AniListUserFeatures.Default;
		features = features & ~AniListUserFeatures.Genres & ~AniListUserFeatures.Mangaka & ~AniListUserFeatures.Studio & ~AniListUserFeatures.MediaFormat;
		var options = (RequestOptions)features;

		var mediaResponse = await _client.SearchMediaAsync(query, type, options, dbUser?.Id, CancellationToken.None);

		if (mediaResponse is null || mediaResponse.Media is null)
		{
			return null;
		}

		var eb = Extensions.CreateMediaBasedThumbnail(mediaResponse.Media, mediaResponse.User?.Options.TitleLanguage ?? TitleLanguage.Default, features)
						   .EnrichWithMediaInfo(mediaResponse.Media, mediaResponse.User, features)
						   .WithImageUrl($"https://img.anili.st/media/{mediaResponse.Media.Id}");

		eb.Thumbnail = null;

		return eb.Build();
	}
}