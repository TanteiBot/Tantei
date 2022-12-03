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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaperMalKing.Database;
using PaperMalKing.Options;

namespace PaperMalKing.Services.Background
{
	public sealed class DiscordBackgroundService : BackgroundService
	{
		private readonly ILogger<DiscordBackgroundService> _logger;
		private readonly IOptions<DiscordOptions> _options;
		private readonly IServiceProvider _provider;
		private readonly DiscordClient Client;

		public DiscordBackgroundService(IServiceProvider provider, IOptions<DiscordOptions> options, ILogger<DiscordBackgroundService> logger,
										DiscordClient client)
		{
			this._logger = logger;

			this._logger.LogTrace("Building {@DiscordBackgroundService}", typeof(DiscordBackgroundService));
			this._provider = provider;
			this._options = options;

			this.Client = client;
			this.Client.Resumed += this.ClientOnResumedAsync;
			this.Client.Ready += this.ClientOnReadyAsync;
			this.Client.ClientErrored += this.ClientOnClientErroredAsync;
			this.Client.GuildMemberRemoved += this.ClientOnGuildMemberRemovedAsync;
			this.Client.GuildDeleted += this.ClientOnGuildDeletedAsync;
			this._logger.LogTrace("Built {@DiscordBackgroundService}", typeof(DiscordBackgroundService));
		}

		private Task ClientOnGuildDeletedAsync(DiscordClient sender, GuildDeleteEventArgs e)
		{
			if (e.Unavailable)
			{
				this._logger.LogInformation("Guild {Guild} became unavailable", e.Guild);
			}
			else
			{
				_ = Task.Factory.StartNew(async () =>
				{
					using var scope = this._provider.CreateScope();
					var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
					var guild = await db.DiscordGuilds.FirstOrDefaultAsync(g => g.DiscordGuildId == e.Guild.Id).ConfigureAwait(false);
					if (guild == null)
					{
						this._logger.LogInformation(
							"Bot was removed from guild {Guild} but since guild wasn't in database there is nothing to remove", e.Guild);
						return;
					}

					db.DiscordGuilds.Remove(guild);
					await db.SaveChangesAndThrowOnNoneAsync().ConfigureAwait(false);
				}, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Current).ContinueWith(
					task => this._logger.LogError(task.Exception, "Task on removing guild from db faulted"), CancellationToken.None,
					TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Current);
			}

			return Task.CompletedTask;
		}

		private Task ClientOnResumedAsync(DiscordClient sender, ReadyEventArgs e)
		{
			this._logger.LogInformation("Discord client resumed");
			return Task.CompletedTask;
		}

		private Task ClientOnReadyAsync(DiscordClient sender, ReadyEventArgs e)
		{
			this._logger.LogInformation("Discord client is ready");
			return Task.CompletedTask;
		}

		private Task ClientOnClientErroredAsync(DiscordClient sender, ClientErrorEventArgs e)
		{
			this._logger.LogError(e.Exception, "Discord client errored");
			return Task.CompletedTask;
		}

		private Task ClientOnGuildMemberRemovedAsync(DiscordClient sender, GuildMemberRemoveEventArgs e)
		{
			_ = Task.Factory.StartNew(async () =>
			{
				using var scope = this._provider.CreateScope();
				var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
				this._logger.LogDebug("User {Member} left guild {Guild}", e.Member, e.Guild);
				var user = await db.DiscordUsers.Include(u => u.Guilds).FirstOrDefaultAsync(u => u.DiscordUserId == e.Member.Id)
								   .ConfigureAwait(false);
				if (user == null)
				{
					this._logger.LogDebug("User {Member} that left wasn't saved in db", e.Member);
				}
				else
				{
					var guild = user.Guilds.FirstOrDefault(g => g.DiscordGuildId == e.Guild.Id);
					if (guild == null)
					{
						this._logger.LogDebug("User {Member} that left guild {Guild} didn't have posting updates in it", e.Member, e.Guild);
						return;
					}

					user.Guilds.Remove(guild);
					db.Update(user);
					await db.SaveChangesAndThrowOnNoneAsync().ConfigureAwait(false);
				}
			}, CancellationToken.None, TaskCreationOptions.None, TaskScheduler.Current).ContinueWith(
				task => this._logger.LogError(task.Exception, "Task on removing left member from the guild failed due to unknown reason"),
				CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Current);
			return Task.CompletedTask;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			this._logger.LogDebug("Starting {@DiscordBackgroundService}", typeof(DiscordBackgroundService));
			this._logger.LogInformation("Connecting to Discord");
			if (this._options.Value.Activities.Count > 1)
			{
				await this.Client.ConnectAsync().ConfigureAwait(false);
				await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken).ConfigureAwait(false);
				_ = Task.Factory.StartNew(async cancellationToken =>
				{
					var token = (CancellationToken) (cancellationToken ?? CancellationToken.None);
					while (!token.IsCancellationRequested)
					{
						foreach (var options in this._options.Value.Activities)
						{
							if (token.IsCancellationRequested)
								return;
							try
							{
								var (discordActivity, userStatus) = this.OptionsToDiscordActivity(options);
								await this.Client.UpdateStatusAsync(discordActivity, userStatus).ConfigureAwait(false);
								await Task.Delay(TimeSpan.FromMilliseconds(options.TimeToBeDisplayedInMilliseconds), token).ConfigureAwait(false);
							}
							catch (TaskCanceledException)
							{
								this._logger.LogInformation("Activity changing canceled");
							}
							#pragma warning disable CA1031
							catch (Exception ex)
							#pragma warning restore CA1031
							{
								this._logger.LogError(ex, "Error occured while updating Discord presence");
							}
						}
					}
				}, stoppingToken, stoppingToken, TaskCreationOptions.None, TaskScheduler.Current).ContinueWith(
					task => this._logger.LogError(task.Exception, "Error occured while updating Discord presence"), CancellationToken.None,
					TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Current);
			}
			else
			{
				this._logger.LogInformation("Found only one Discord status in options so it won't be changed");
				var (discordActivity, userStatus) = this.OptionsToDiscordActivity(this._options.Value.Activities[0]);
				await this.Client.ConnectAsync(discordActivity, userStatus).ConfigureAwait(false);
			}

			await Task.Delay(Timeout.Infinite, stoppingToken).ConfigureAwait(false);
			var t = this.Client.DisconnectAsync();
			this._logger.LogInformation("Disconnecting from Discord");
			await t.ConfigureAwait(false);
		}

		private (DiscordActivity, UserStatus) OptionsToDiscordActivity(DiscordOptions.DiscordActivityOptions options)
		{
			if (!Enum.TryParse(options.ActivityType, true, out ActivityType activityType))
			{
				var correctActivities = string.Join(", ", Enum.GetValues<ActivityType>());
				this._logger.LogError("Couldn't parse correct ActivityType from {ActivityType}, correct values are {CorrectActivities}",
					options.ActivityType, correctActivities);
				activityType = ActivityType.Playing;
			}

			if (!Enum.TryParse(options.Status, true, out UserStatus status))
			{
				var correctStatuses = string.Join(", ", Enum.GetValues<UserStatus>());
				this._logger.LogError("Couldn't parse correct UserStatus from {Status}, correct values are {CorrectStatuses}", options.Status,
					correctStatuses);
				status = UserStatus.Online;
			}

			return (new(options.PresenceText, activityType), status);
		}

		/// <inheritdoc />
		public override void Dispose()
		{
			base.Dispose();
			this._logger.LogDebug("Disposing {@DiscordBackgroundService}", typeof(DiscordBackgroundService));
			this.Client.Dispose();
		}
	}
}