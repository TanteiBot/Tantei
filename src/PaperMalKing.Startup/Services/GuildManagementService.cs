// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaperMalKing.Database;
using PaperMalKing.Database.Models;
using PaperMalKing.Startup.Exceptions;

namespace PaperMalKing.Startup.Services;

internal sealed class GuildManagementService
{
	private readonly ILogger<GuildManagementService> _logger;
	private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

	private readonly UpdatePublishingService _updatePublishingService;
	private readonly DiscordClient _discordClient;

	[SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates", Justification = "It's meant to be a singleton")]
	public GuildManagementService(ILogger<GuildManagementService> logger, IDbContextFactory<DatabaseContext> dbContextFactory, UpdatePublishingService updatePublishingService, DiscordClient discordClient)
	{
		this._logger = logger;
		this._dbContextFactory = dbContextFactory;
		this._logger.LogTrace("Building {@GuildManagementService}", typeof(GuildManagementService));
		this._updatePublishingService = updatePublishingService;
		this._discordClient = discordClient;
		this._logger.LogTrace("Built {@GuildManagementService}", typeof(GuildManagementService));
	}

	public async Task<DiscordGuild> SetChannelAsync(ulong guildId, ulong channelId)
	{
		await using var db = this._dbContextFactory.CreateDbContext();
		var guild = db.DiscordGuilds.TagWith("Query guild to set a channel for it").TagWithCallSite().FirstOrDefault(g => g.DiscordGuildId == guildId);
		if (guild != null)
		{
			throw new GuildManagementException("Server already have channel to post updates into", guildId, channelId);
		}

		var dGuild = this._discordClient.Guilds[guildId];
		this._logger.SettingChannelForGuild(dGuild, dGuild.Channels[channelId]);

		guild = new()
		{
			DiscordGuildId = guildId,
			PostingChannelId = channelId,
			Users = [],
		};
		db.DiscordGuilds.Add(guild);
		await db.SaveChangesAndThrowOnNoneAsync();
		var chn = await this._discordClient.GetChannelAsync(channelId);
		this._updatePublishingService.AddChannel(chn);
		return guild;
	}

	public async Task RemoveGuildAsync(ulong guildId)
	{
		await using var db = this._dbContextFactory.CreateDbContext();
		var guild = db.DiscordGuilds.TagWith("Query guild to remove it").TagWithCallSite().FirstOrDefault() ??
					throw new GuildManagementException("You can't remove this server from posting updates", guildId);
		this._logger.RemovingChannel(guildId);

		db.DiscordGuilds.Where(g => g.DiscordGuildId == guildId).ExecuteDelete();
		db.DiscordGuilds.Remove(guild);
		this._updatePublishingService.RemoveChannel(guild.PostingChannelId);
		await db.SaveChangesAndThrowOnNoneAsync();
	}

	public async Task UpdateChannelAsync(ulong guildId, ulong channelId)
	{
		await using var db = this._dbContextFactory.CreateDbContext();
		var guild =
			db.DiscordGuilds.TagWith("Query guild to update channel for it").TagWithCallSite().FirstOrDefault(g => g.DiscordGuildId == guildId) ??
			throw new GuildManagementException("You can't update channel for posting updates without setting it first", guildId, channelId);
		if (guild.PostingChannelId == channelId)
		{
			throw new GuildManagementException("You can't update channel to the same channel", guildId, channelId);
		}

		var discordGuild = this._discordClient.Guilds[guildId];
		discordGuild.Channels.TryGetValue(guild.PostingChannelId, out var currentChannel);
		discordGuild.Channels.TryGetValue(channelId, out var newChannel);
		this._logger.UpdatingChannel(
			this._discordClient.Guilds[guildId],
			currentChannel?.Id ?? guild.PostingChannelId,
			newChannel?.Id ?? channelId);
		var opci = guild.PostingChannelId;
		guild.PostingChannelId = channelId;
		await db.SaveChangesAndThrowOnNoneAsync();
		this._updatePublishingService.UpdateChannel(opci, await this._discordClient.GetChannelAsync(channelId));
	}
}