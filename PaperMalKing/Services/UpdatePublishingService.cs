#region LICENSE

// PaperMalKing.
// Copyright (C) 2021 N0D4N
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PaperMalKing.Data;
using PaperMalKing.Database;
using PaperMalKing.UpdatesProviders.Base;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;

namespace PaperMalKing.Services
{
	public sealed class UpdatePublishingService
	{
		private readonly ILogger<UpdatePublishingService> _logger;
		private readonly DiscordClient _discordClient;
		private readonly IServiceProvider _serviceProvider;
		private readonly UpdateProvidersConfigurationService _updateProvidersConfigurationService;
		private readonly ConcurrentDictionary<ulong, UpdatePoster> _updatePosters = new();

		public UpdatePublishingService(ILogger<UpdatePublishingService> logger, DiscordClient discordClient, IServiceProvider serviceProvider,
									   UpdateProvidersConfigurationService updateProvidersConfigurationService)
		{
			this._logger = logger;
			this._logger.LogTrace("Building {@UpdatePublishingService}", typeof(UpdatePublishingService));

			this._discordClient = discordClient;
			this._serviceProvider = serviceProvider;
			this._updateProvidersConfigurationService = updateProvidersConfigurationService;
			this._discordClient.GuildDownloadCompleted += this.DiscordClientOnGuildDownloadCompleted;
			this._logger.LogTrace("Built {@UpdatePublishingService}", typeof(UpdatePublishingService));
		}

		private Task DiscordClientOnGuildDownloadCompleted(DiscordClient sender, GuildDownloadCompletedEventArgs e)
		{
			_ = Task.Run(async () =>
			{
				using var scope = this._serviceProvider.CreateScope();
				var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
				this._logger.LogDebug("Starting querying posting channels");
				await foreach (var guild in db.DiscordGuilds.AsNoTracking().AsAsyncEnumerable().ConfigureAwait(false))
				{
					this._logger.LogTrace("Trying to get guild with {Id}", guild.DiscordGuildId);
					var discordGuild = e.Guilds[guild.DiscordGuildId];
					this._logger.LogTrace(@"Loaded guild {Guild}", discordGuild);
					var channel = discordGuild.GetChannel(guild.PostingChannelId) ??
								  (await discordGuild.GetChannelsAsync().ConfigureAwait(false)).First(ch => ch.Id == guild.PostingChannelId);
					this._logger.LogTrace("Loaded channel {Channel} in guild {DiscordGuild}", channel, discordGuild);
					this.AddChannel(channel);
				}

				#pragma warning disable S3267
				foreach (var kvp in this._updateProvidersConfigurationService.Providers)
				#pragma warning restore S3267
				{
					var provider = kvp.Value;
					provider.UpdateFoundEvent += this.PublishUpdates;
					if (provider is BaseUpdateProvider baseUpdateProvider)
						baseUpdateProvider.RestartTimer(TimeSpan.FromSeconds(5));
				}

				this._logger.LogDebug("Ended querying posting channels");
				this._discordClient.GuildDownloadCompleted -= this.DiscordClientOnGuildDownloadCompleted;
			}).ContinueWith(task => this._logger.LogError(task.Exception, "Task on loading channels to post to failed"), CancellationToken.None,
				TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Current);
			return Task.CompletedTask;
		}


		public void RemoveChannel(ulong id) => this._updatePosters.TryRemove(id, out _);

		public bool AddChannel(DiscordChannel channel) => this._updatePosters.TryAdd(channel.Id,
			new(this._serviceProvider.GetRequiredService<ILogger<UpdatePoster>>(), channel));

		public void UpdateChannel(ulong key, DiscordChannel updatedValue)
		{
			this._updatePosters.Remove(key, out _);
			this.AddChannel(updatedValue);
		}

		private async Task PublishUpdates(UpdateFoundEventArgs args)
		{
			using var scope = this._serviceProvider.CreateScope();
			var tasks = new List<Task>(args.DiscordUser.Guilds.Count);
			foreach (var guild in args.DiscordUser.Guilds)
			{
				tasks.Add(this._updatePosters[guild.PostingChannelId].PostUpdatesAsync(args.Update.UpdateEmbeds));
			}

			await Task.WhenAll(tasks).ConfigureAwait(false);
		}
	}
}