// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
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

	public GuildManagementService(ILogger<GuildManagementService> logger, IDbContextFactory<DatabaseContext> dbContextFactory,
								  UpdatePublishingService updatePublishingService, DiscordClient discordClient)
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
		using var db = this._dbContextFactory.CreateDbContext();
		var guild = db.DiscordGuilds.TagWith("Query guild to set a channel for it").TagWithCallSite().FirstOrDefault(g => g.DiscordGuildId == guildId);
		if (guild != null)
			throw new GuildManagementException("Server already have channel to post updates into", guildId, channelId);
		this._logger.LogInformation("Setting channel for guild {Guild} at {Channel}", this._discordClient.Guilds[guildId],
			this._discordClient.Guilds[guildId].Channels[channelId]);

		guild = new()
		{
			DiscordGuildId = guildId,
			PostingChannelId = channelId,
			Users = Array.Empty<DiscordUser>(),
		};
		db.DiscordGuilds.Add(guild);
		await db.SaveChangesAndThrowOnNoneAsync().ConfigureAwait(false);
		var chn = await this._discordClient.GetChannelAsync(channelId).ConfigureAwait(false);
		this._updatePublishingService.AddChannel(chn);
		return guild;
	}

	public async Task RemoveGuildAsync(ulong guildId)
	{
		using var db = this._dbContextFactory.CreateDbContext();
		var guild = db.DiscordGuilds.TagWith("Query guild to remove it").TagWithCallSite().FirstOrDefault() ?? throw new GuildManagementException("You can't remove this server from posting updates", guildId);
		this._logger.LogInformation("Removing guild with {Id}", guildId);

		db.DiscordGuilds.Where(g => g.DiscordGuildId == guildId).ExecuteDelete();
		db.DiscordGuilds.Remove(guild);
		this._updatePublishingService.RemoveChannel(guild.PostingChannelId);
		await db.SaveChangesAndThrowOnNoneAsync().ConfigureAwait(false);
	}

	public async Task UpdateChannelAsync(ulong guildId, ulong channelId)
	{
		using var db = this._dbContextFactory.CreateDbContext();
		var guild = db.DiscordGuilds.TagWith("Query guild to update channel for it").TagWithCallSite().FirstOrDefault(g => g.DiscordGuildId == guildId) ?? throw new GuildManagementException("You can't update channel for posting updates without setting it first", guildId, channelId);
		if (guild.PostingChannelId == channelId)
			throw new GuildManagementException("You can't update channel to the same channel", guildId, channelId);

		var discordGuild = this._discordClient.Guilds[guildId];
		discordGuild.Channels.TryGetValue(guild.PostingChannelId, out var currentChannel);
		discordGuild.Channels.TryGetValue(channelId, out var newChannel);
		this._logger.LogInformation("Updating channel of {Guild} from {CurrentChannel} to {NewChannel}", this._discordClient.Guilds[guildId],
			currentChannel ?? (object)guild.PostingChannelId, newChannel ?? (object)channelId);
		var opci = guild.PostingChannelId;
		guild.PostingChannelId = channelId;
		await db.SaveChangesAndThrowOnNoneAsync().ConfigureAwait(false);
		this._updatePublishingService.UpdateChannel(opci, await this._discordClient.GetChannelAsync(channelId).ConfigureAwait(false));
	}
}