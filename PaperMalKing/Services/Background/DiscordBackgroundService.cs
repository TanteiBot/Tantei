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
		private readonly IOptions<DiscordOptions> _options;
		private readonly IServiceProvider _provider;
		private readonly ILogger<DiscordBackgroundService> _logger;
		public readonly DiscordClient Client;

		public DiscordBackgroundService(IServiceProvider provider, IOptions<DiscordOptions> options, ILogger<DiscordBackgroundService> logger,
										DiscordClient client)
		{
			this._logger = logger;

			this._logger.LogTrace($"Building {nameof(DiscordBackgroundService)}");
			this._provider = provider;
			this._options = options;

			this.Client = client;
			this.Client.Resumed += this.ClientOnResumed;
			this.Client.Ready += this.ClientOnReady;
			this.Client.ClientErrored += this.ClientOnClientErrored;
			this.Client.GuildMemberRemoved += this.ClientOnGuildMemberRemoved;
			this.Client.GuildDeleted += this.ClientOnGuildDeleted;
			this._logger.LogTrace($"Built {nameof(DiscordBackgroundService)}");
		}

		private Task ClientOnGuildDeleted(DiscordClient sender, GuildDeleteEventArgs e)
		{
			if (e.Unavailable)
			{
				this._logger.LogInformation($"Guild {e.Guild} became unavailable");
				return Task.CompletedTask;
			}

			_ = Task.Factory.StartNew(async () =>
			{
				using var scope = this._provider.CreateScope();
				var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
				var guild = await db.DiscordGuilds.FirstOrDefaultAsync(g => g.DiscordGuildId == e.Guild.Id);
				if (guild == null)
				{
					this._logger.LogInformation(
						$"Bot was removed from guild {e.Guild} but since guild wasn't in database there is nothing to remove");
					return;
				}

				db.DiscordGuilds.Remove(guild);
				await db.SaveChangesAndThrowOnNoneAsync();
			}).ContinueWith(task => this._logger.LogError(task.Exception, "Task on removing guild from db faulted"),
				TaskContinuationOptions.OnlyOnFaulted);

			return Task.CompletedTask;
		}

		private Task ClientOnResumed(DiscordClient sender, ReadyEventArgs e)
		{
			this._logger.LogInformation("Discord client resumed");
			return Task.CompletedTask;
		}

		private Task ClientOnReady(DiscordClient sender, ReadyEventArgs e)
		{
			this._logger.LogInformation("Discord client is ready");
			return Task.CompletedTask;
		}

		private Task ClientOnClientErrored(DiscordClient sender, ClientErrorEventArgs e)
		{
			this._logger.LogError(e.Exception, "Discord client errored");
			return Task.CompletedTask;
		}

		private Task ClientOnGuildMemberRemoved(DiscordClient sender, GuildMemberRemoveEventArgs e)
		{
			_ = Task.Factory.StartNew(async () =>
			{
				using var scope = this._provider.CreateScope();
				var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
				this._logger.LogDebug($"User {e.Member} left guild {e.Guild}");
				var user = await db.DiscordUsers.Include(u => u.Guilds).FirstOrDefaultAsync(u => u.DiscordUserId == e.Member.Id);
				if (user == null)
				{
					this._logger.LogDebug($"User {e.Member} that left wasn't saved in db");
					return;
				}
				else
				{
					var guild = user.Guilds.FirstOrDefault(g => g.DiscordGuildId == e.Guild.Id);
					if (guild == null)
					{
						this._logger.LogDebug($"User {e.Member} that left guild {e.Guild} didn't have posting updates in it");
						return;
					}

					user.Guilds.Remove(guild);
					db.Update(user);
					await db.SaveChangesAndThrowOnNoneAsync();
				}
			}).ContinueWith(task => this._logger.LogError(task.Exception, "Task on removing left member from the guild failed due to unknown reason"),
				TaskContinuationOptions.OnlyOnFaulted);
			return Task.CompletedTask;
		}

		/// <inheritdoc />
		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			this._logger.LogDebug($"Starting {nameof(DiscordBackgroundService)}");
			this._logger.LogInformation("Connecting to Discord");
			await this.Client.ConnectAsync(new(this._options.Value.PresenceText, (ActivityType) this._options.Value.ActivityType), UserStatus.Online);
			await Task.Delay(Timeout.Infinite, stoppingToken);
			var t = this.Client.DisconnectAsync();
			this._logger.LogInformation("Disconnecting from Discord");
			await t;
		}

		/// <inheritdoc />
		public override void Dispose()
		{
			this._logger.LogDebug($"Disposing {nameof(DiscordBackgroundService)}");
			this.Client.Dispose();
		}
	}
}