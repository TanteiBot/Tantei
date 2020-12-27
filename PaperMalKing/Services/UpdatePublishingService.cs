using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
			this._logger.LogTrace($"Building {nameof(UpdatePublishingService)}");

			this._discordClient = discordClient;
			this._serviceProvider = serviceProvider;
			this._updateProvidersConfigurationService = updateProvidersConfigurationService;
			this._discordClient.GuildDownloadCompleted += this.DiscordClientOnGuildDownloadCompleted;
			this._logger.LogTrace($"Built {nameof(UpdatePublishingService)}");
		}

		private Task DiscordClientOnGuildDownloadCompleted(DiscordClient sender, GuildDownloadCompletedEventArgs e)
		{
			_ = Task.Run(async () =>
			{
				using var scope = this._serviceProvider.CreateScope();
				var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
				this._logger.LogDebug("Starting querying posting channels");
				await foreach (var guild in db.DiscordGuilds.AsNoTracking().AsAsyncEnumerable())
				{
					var discordGuild = await sender.GetGuildAsync(guild.DiscordGuildId);
					this._logger.LogTrace($"Loaded guild {discordGuild.Name} {discordGuild.Id.ToString()}");
					var channels = await discordGuild.GetChannelsAsync();
					var channel = channels.First(ch => ch.Id == guild.PostingChannelId);
					this._logger.LogTrace($"Loaded channel {channel.Id.ToString()} in guild {discordGuild.Id.ToString()}");
					this.AddChannel(channel);
				}

				foreach (var kvp in this._updateProvidersConfigurationService.Providers)
				{
					var provider = kvp.Value;
					provider.UpdateFoundEvent += this.PublishUpdates;
					if (provider is BaseUpdateProvider baseUpdateProvider)
						baseUpdateProvider.RestartTimer(TimeSpan.FromSeconds(5));
				}

				this._logger.LogDebug("Ended querying posting channels");
				this._discordClient.GuildDownloadCompleted -= this.DiscordClientOnGuildDownloadCompleted;
			}).ContinueWith(task => this._logger.LogError(task.Exception, "Task on loading channels to post to failed"),
				TaskContinuationOptions.OnlyOnFaulted);
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

			await Task.WhenAll(tasks);
		}
	}
}