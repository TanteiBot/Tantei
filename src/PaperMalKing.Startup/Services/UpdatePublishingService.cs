// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PaperMalKing.Database;
using PaperMalKing.Startup.Data;
using PaperMalKing.Startup.Exceptions;
using PaperMalKing.UpdatesProviders.Base;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;

namespace PaperMalKing.Startup.Services;

internal sealed class UpdatePublishingService
{
	private readonly ILogger<UpdatePublishingService> _logger;
	private readonly DiscordClient _discordClient;
	private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
	private readonly UpdateProvidersConfigurationService _updateProvidersConfigurationService;
	private readonly ILoggerFactory _loggerFactory;
	private readonly ConcurrentDictionary<ulong, UpdatePoster> _updatePosters = new();

	[SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates", Justification = "It's meant to be a singleton")]
	public UpdatePublishingService(ILogger<UpdatePublishingService> logger, DiscordClient discordClient, IDbContextFactory<DatabaseContext> dbContextFactory, UpdateProvidersConfigurationService updateProvidersConfigurationService, ILoggerFactory loggerFactory)
	{
		this._logger = logger;
		this._logger.LogTrace("Building {@UpdatePublishingService}", typeof(UpdatePublishingService));

		this._discordClient = discordClient;
		this._dbContextFactory = dbContextFactory;
		this._updateProvidersConfigurationService = updateProvidersConfigurationService;
		this._loggerFactory = loggerFactory;
		this._discordClient.GuildDownloadCompleted += this.DiscordClientOnGuildDownloadCompletedAsync;
		this._logger.LogTrace("Built {@UpdatePublishingService}", typeof(UpdatePublishingService));
	}

	private Task DiscordClientOnGuildDownloadCompletedAsync(DiscordClient sender, GuildDownloadCompletedEventArgs e)
	{
		_ = Task.Run(async () =>
		{
			await using var db = this._dbContextFactory.CreateDbContext();
			this._logger.LogDebug("Starting querying posting channels");
			foreach (var guild in db.DiscordGuilds.TagWith("Query guild to save posting channels").TagWithCallSite().AsNoTracking().ToArray())
			{
				this._logger.LogTrace("Trying to get guild with {Id}", guild.DiscordGuildId);
				var discordGuild = e.Guilds[guild.DiscordGuildId];
				this._logger.LogTrace("Loaded guild {Guild}", discordGuild);
				#pragma warning disable EA0013
				// Consider removing unnecessary null coalescing (??) since the left-hand value is statically known not to be null
				var channel = discordGuild.GetChannel(guild.PostingChannelId) ??
							  (await discordGuild.GetChannelsAsync()).FirstOrDefault(ch => ch.Id == guild.PostingChannelId);
				#pragma warning restore EA0013
				this._logger.LogTrace("Loaded channel {Channel} in guild {DiscordGuild}", channel, discordGuild);
				if (channel is not null)
				{
					this.AddChannel(channel);
				}
			}

			foreach (var provider in this._updateProvidersConfigurationService.Providers.Values)
			{
				provider.UpdateFoundEvent += this.PublishUpdatesAsync;
				if (provider is BaseUpdateProvider baseUpdateProvider)
				{
					baseUpdateProvider.RestartTimer(TimeSpan.FromSeconds(5));
				}
			}

			this._logger.LogDebug("Ended querying posting channels");
			this._discordClient.GuildDownloadCompleted -= this.DiscordClientOnGuildDownloadCompletedAsync;
		}).ContinueWith(task => this._logger.LogError(task.Exception, "Task on loading channels to post to failed"), CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Current);
		return Task.CompletedTask;
	}

	public void RemoveChannel(ulong id)
	{
		if (this._updatePosters.TryRemove(id, out var updatePoster))
		{
			updatePoster.Dispose();
		}
		else
		{
			throw new GuildManagementException("Couldnt remove or update channel");
		}
	}

	public bool AddChannel(DiscordChannel channel) => this._updatePosters.TryAdd(channel.Id, new(this._loggerFactory.CreateLogger<UpdatePoster>(), channel));

	public void UpdateChannel(ulong key, DiscordChannel updatedValue)
	{
		this.RemoveChannel(key);

		this.AddChannel(updatedValue);
	}

	private Task PublishUpdatesAsync(UpdateFoundEventArgs args)
	{
		var tasks = new List<Task>(args.DiscordUser.Guilds.Count);
		foreach (var guild in args.DiscordUser.Guilds)
		{
			tasks.Add(this._updatePosters[guild.PostingChannelId].PostUpdatesAsync(args.Update.UpdateEmbeds));
		}

		return Task.WhenAll(tasks);
	}
}