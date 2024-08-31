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

	public DiscordBackgroundService(IOptions<DiscordOptions> options,
									ILogger<DiscordBackgroundService> logger,
									DiscordClient client,
									IDbContextFactory<DatabaseContext> dbContextFactory,
									GuildManagementService guildManagementService,
									GeneralUserService userService)
	{
		this._logger = logger;
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
	}

	private Task ClientOnGuildDeletedAsync(DiscordClient sender, GuildDeleteEventArgs e)
	{
		if (e.Unavailable)
		{
			this._logger.GuildBecameUnavailable(e.Guild);
		}
		else
		{
			_ = Task.Factory.StartNew(
				async () =>
				{
					this._logger.BotWasRemovedFromGuild(e.Guild);
					await this._guildManagementService.RemoveGuildAsync(e.Guild.Id);
				},
				CancellationToken.None,
				TaskCreationOptions.None,
				TaskScheduler.Current).ContinueWith(
				task => this._logger.RemovingGuildFromDbFailed(task.Exception), CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Current);
		}

		return Task.CompletedTask;
	}

	private Task ClientOnResumedAsync(DiscordClient sender, ReadyEventArgs e)
	{
		this._logger.DiscordClientResumed();
		return Task.CompletedTask;
	}

	private Task ClientOnReadyAsync(DiscordClient sender, ReadyEventArgs e)
	{
		this._logger.DiscordClientReady();
		return Task.CompletedTask;
	}

	private Task ClientOnClientErroredAsync(DiscordClient sender, ClientErrorEventArgs e)
	{
		this._logger.DiscordClientErrored(e.Exception);
		return Task.CompletedTask;
	}

	[SuppressMessage("Roslynator", "RCS1261:Resource can be disposed asynchronously", Justification = "Sqlite does not support async")]
	private Task ClientOnGuildMemberRemovedAsync(DiscordClient sender, GuildMemberRemoveEventArgs e)
	{
		_ = Task.Factory.StartNew(
			async () =>
		{
			using var db = this._dbContextFactory.CreateDbContext();
			this._logger.UserLeftGuild(e.Member, e.Guild);
			var isUserInDb = db.DiscordUsers.TagWith("Check for users presence in DB when member leaves").TagWithCallSite()
							   .Any(u => u.DiscordUserId == e.Member.Id);
			if (!isUserInDb)
			{
				this._logger.UserThatLeftWasNotInDb(e.Member);
			}
			else
			{
				await this._userService.RemoveUserInGuildAsync(e.Guild.Id, e.Member.Id);
			}
		},
			CancellationToken.None,
			TaskCreationOptions.None,
			TaskScheduler.Current).ContinueWith(task => this._logger.TaskOnRemovingUserFailed(task.Exception), CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Current);
		return Task.CompletedTask;
	}

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		this._logger.ConnectingToDiscord();
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
						catch (TaskCanceledException)
						{
							// Ignore
						}
						#pragma warning disable CA1031
						// Modify 'ExecuteAsync' to catch a more specific allowed exception type, or rethrow the exception
						catch (Exception ex)
							#pragma warning restore CA1031
						{
							this._logger.ErrorOccuredWhileChangingDiscordPresence(ex);
						}
					}
				}
			},
				stoppingToken,
				stoppingToken,
				TaskCreationOptions.None,
				TaskScheduler.Current).ContinueWith(
				task => this._logger.ErrorOccuredWhileChangingDiscordPresence(task.Exception), CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Current);
		}
		else
		{
			this._logger.NoStatusWouldBeChanged();
			var (discordActivity, userStatus) = this.OptionsToDiscordActivity(this._options.Value.Activities[0]);
			await this._client.ConnectAsync(discordActivity, userStatus);
		}

		await Task.Delay(Timeout.Infinite, stoppingToken);
		var t = this._client.DisconnectAsync();
		this._logger.DisconnectingFromDiscord();
		await t;
	}

	private (DiscordActivity Activity, UserStatus Status) OptionsToDiscordActivity(DiscordOptions.DiscordActivityOptions options)
	{
		if (!Enum.TryParse(options.ActivityType, ignoreCase: true, out ActivityType activityType))
		{
			var correctActivities = string.Join(", ", Enum.GetValues<ActivityType>());
			this._logger.CouldNotParseCorrectActivity(options.ActivityType, correctActivities);
			activityType = ActivityType.Playing;
		}

		if (Enum.TryParse(options.Status, ignoreCase: true, out UserStatus status))
		{
			return (new(options.PresenceText, activityType), status);
		}

		var correctStatuses = string.Join(", ", Enum.GetValues<UserStatus>());
		this._logger.CouldNotParseCorrectStatus(options.Status, correctStatuses);
		status = UserStatus.Online;

		return (new(options.PresenceText, activityType), status);
	}

	public override void Dispose()
	{
		base.Dispose();
		this._client.Dispose();
	}
}