// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

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

	public UpdatePublishingService(ILogger<UpdatePublishingService> logger, DiscordClient discordClient, IDbContextFactory<DatabaseContext> dbContextFactory, UpdateProvidersConfigurationService updateProvidersConfigurationService, ILoggerFactory loggerFactory)
	{
		this._logger = logger;
		this._discordClient = discordClient;
		this._dbContextFactory = dbContextFactory;
		this._updateProvidersConfigurationService = updateProvidersConfigurationService;
		this._loggerFactory = loggerFactory;
		this._discordClient.GuildDownloadCompleted += this.DiscordClientOnGuildDownloadCompletedAsync;
	}

	[SuppressMessage("Roslynator", "RCS1261:Resource can be disposed asynchronously", Justification = "Sqlite does not support async")]
	private Task DiscordClientOnGuildDownloadCompletedAsync(DiscordClient sender, GuildDownloadCompletedEventArgs e)
	{
		_ = Task.Run(async () =>
		{
			using var db = this._dbContextFactory.CreateDbContext();
			this._logger.StartingQueryingPostingChannels();
			foreach (var guild in db.DiscordGuilds.TagWith("Query guild to save posting channels").TagWithCallSite().AsNoTracking().ToArray())
			{
				this._logger.TryingToGetGuildWithId(guild.DiscordGuildId);
				var discordGuild = e.Guilds[guild.DiscordGuildId];
				this._logger.LoadedGuild(discordGuild);
				#pragma warning disable EA0013
				// Consider removing unnecessary null coalescing (??) since the left-hand value is statically known not to be null
				var channel = discordGuild.GetChannel(guild.PostingChannelId) ??
							  (await discordGuild.GetChannelsAsync()).FirstOrDefault(ch => ch.Id == guild.PostingChannelId);
				#pragma warning restore EA0013
				this._logger.LoadedChannelInGuild(channel, discordGuild);
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
					baseUpdateProvider.StartOrRestartAfter(TimeSpan.FromSeconds(5));
				}
			}

			this._logger.EndedQueryingPostingChannels();
			this._discordClient.GuildDownloadCompleted -= this.DiscordClientOnGuildDownloadCompletedAsync;
		}).ContinueWith(task => this._logger.LoadingChannelsToPostFailed(task.Exception), CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Current);
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
			throw new GuildManagementException("Couldn't remove or update channel");
		}
	}

	public bool AddChannel(DiscordChannel channel) => this._updatePosters.TryAdd(channel.Id, new(this._loggerFactory.CreateLogger<UpdatePoster>(), channel));

	public void UpdateChannel(ulong key, DiscordChannel updatedValue)
	{
		this.RemoveChannel(key);

		this.AddChannel(updatedValue);
	}

	private async Task PublishUpdatesAsync(object? sender, UpdateFoundEventArgs args)
	{
		var tasks = new List<Task>(args.DiscordUser.Guilds.Count);

		try
		{
			await Task.WhenAll(args.DiscordUser.Guilds.Select(g => g.PostingChannelId).Select(i => this._updatePosters[i].PreparePostingUpdatesAsync()));

			await foreach (var embed in args.Update.GetUpdateEmbedsAsync())
			{
				tasks.AddRange(args.DiscordUser.Guilds.Select(guild => this._updatePosters[guild.PostingChannelId].PostUpdateAsync(embed)));

				await Task.WhenAll(tasks);

				tasks.Clear();
			}
		}
		finally
		{
			foreach (var guild in args.DiscordUser.Guilds)
			{
				this._updatePosters[guild.PostingChannelId].FinishPostingUpdates();
			}
		}
	}
}