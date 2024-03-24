// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaperMalKing.Database;
using PaperMalKing.Startup.Options;
using PaperMalKing.UpdatesProviders.Base;

namespace PaperMalKing.Startup.Services.Background;

internal sealed class DiscordBackgroundService : BackgroundService
{
	private readonly ILogger<DiscordBackgroundService> _logger;
	private readonly IOptions<DiscordOptions> _options;
	private readonly DiscordClient _client;
	private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
	private readonly GuildManagementService _guildManagementService;
	private readonly GeneralUserService _userService;

	[SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates", Justification = "It's meant to be a singleton")]
	public DiscordBackgroundService(
									IOptions<DiscordOptions> options,
									ILogger<DiscordBackgroundService> logger,
									DiscordClient client,
									IDbContextFactory<DatabaseContext> dbContextFactory,
									GuildManagementService guildManagementService,
									GeneralUserService userService)
	{
		this._logger = logger;

		this._logger.LogTrace("Building {@DiscordBackgroundService}", typeof(DiscordBackgroundService));
		this._options = options;

		this._client = client;
		this._dbContextFactory = dbContextFactory;
		this._guildManagementService = guildManagementService;
		this._userService = userService;
		this._client.Resumed += this.ClientOnResumedAsync;
		this._client.Ready += this.ClientOnReadyAsync;
		this._client.ClientErrored += this.ClientOnClientErroredAsync;
		this._client.GuildMemberRemoved += this.ClientOnGuildMemberRemovedAsync;
		this._client.GuildDeleted += this.ClientOnGuildDeletedAsync;
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
			_ = Task.Factory.StartNew(
				async () =>
				{
					this._logger.LogInformation("Bot was removed from {Guild}", e.Guild);
					await this._guildManagementService.RemoveGuildAsync(e.Guild.Id);
				},
				CancellationToken.None,
				TaskCreationOptions.None,
				TaskScheduler.Current).ContinueWith(
				task => this._logger.LogError(task.Exception, "Task on removing guild from db faulted"), CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Current);
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
		_ = Task.Factory.StartNew(
			async () =>
		{
			await using var db = this._dbContextFactory.CreateDbContext();
			this._logger.LogDebug("User {Member} left guild {Guild}", e.Member, e.Guild);
			var isUserInDb = db.DiscordUsers.TagWith("Check for users presence in DB when member leaves").TagWithCallSite()
							   .Any(u => u.DiscordUserId == e.Member.Id);
			if (!isUserInDb)
			{
				this._logger.LogDebug("User {Member} that left wasn't saved in db", e.Member);
			}
			else
			{
				await this._userService.RemoveUserInGuildAsync(e.Guild.Id, e.Member.Id);
			}
		},
			CancellationToken.None,
			TaskCreationOptions.None,
			TaskScheduler.Current).ContinueWith(
			task => this._logger.LogError(task.Exception, "Task on removing left member from the guild failed due to unknown reason"),
			CancellationToken.None,
			TaskContinuationOptions.OnlyOnFaulted,
			TaskScheduler.Current);
		return Task.CompletedTask;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		this._logger.LogDebug("Starting {@DiscordBackgroundService}", typeof(DiscordBackgroundService));
		this._logger.LogInformation("Connecting to Discord");
		if (this._options.Value.Activities is not [])
		{
			await this._client.ConnectAsync();
			await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
			_ = Task.Factory.StartNew(
				async cancellationToken =>
			{
				var token = (CancellationToken)(cancellationToken ?? CancellationToken.None);
				while (!token.IsCancellationRequested)
				{
					foreach (var options in this._options.Value.Activities)
					{
						if (token.IsCancellationRequested)
						{
							return;
						}

						try
						{
							var (discordActivity, userStatus) = this.OptionsToDiscordActivity(options);
							await this._client.UpdateStatusAsync(discordActivity, userStatus);
							await Task.Delay(TimeSpan.FromMilliseconds(options.TimeToBeDisplayedInMilliseconds), token);
						}
						catch (TaskCanceledException e)
						{
							this._logger.LogInformation(e, "Activity changing canceled");
						}
						#pragma warning disable CA1031
						// Modify 'ExecuteAsync' to catch a more specific allowed exception type, or rethrow the exception
						catch (Exception ex)
							#pragma warning restore CA1031
						{
							this._logger.LogError(ex, "Error occured while updating Discord presence");
						}
					}
				}
			},
				stoppingToken,
				stoppingToken,
				TaskCreationOptions.None,
				TaskScheduler.Current).ContinueWith(
				task => this._logger.LogError(task.Exception, "Error occured while updating Discord presence"), CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Current);
		}
		else
		{
			this._logger.LogInformation("Found only one Discord status in options so it won't be changed");
			var (discordActivity, userStatus) = this.OptionsToDiscordActivity(this._options.Value.Activities[0]);
			await this._client.ConnectAsync(discordActivity, userStatus);
		}

		await Task.Delay(Timeout.Infinite, stoppingToken);
		var t = this._client.DisconnectAsync();
		this._logger.LogInformation("Disconnecting from Discord");
		await t;
	}

	private (DiscordActivity Activity, UserStatus Status) OptionsToDiscordActivity(DiscordOptions.DiscordActivityOptions options)
	{
		if (!Enum.TryParse(options.ActivityType, ignoreCase: true, out ActivityType activityType))
		{
			var correctActivities = string.Join(", ", Enum.GetValues<ActivityType>());
			this._logger.LogError(
				"Couldn't parse correct ActivityType from {ActivityType}, correct values are {CorrectActivities}",
				options.ActivityType,
				correctActivities);
			activityType = ActivityType.Playing;
		}

		if (!Enum.TryParse(options.Status, ignoreCase: true, out UserStatus status))
		{
			var correctStatuses = string.Join(", ", Enum.GetValues<UserStatus>());
			this._logger.LogError(
				"Couldn't parse correct UserStatus from {Status}, correct values are {CorrectStatuses}",
				options.Status,
				correctStatuses);
			status = UserStatus.Online;
		}

		return (new(options.PresenceText, activityType), status);
	}

	[SuppressMessage("Performance", "CA1848:Use the LoggerMessage delegates", Justification = "It's meant to be called once")]
	public override void Dispose()
	{
		base.Dispose();
		this._logger.LogDebug("Disposing {@DiscordBackgroundService}", typeof(DiscordBackgroundService));
		this._client.Dispose();
	}
}