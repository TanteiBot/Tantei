/*
 * Cleaned up a bit should look nice now
 */
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CodeHollow.FeedReader;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using PaperMalKing.Data;
using PaperMalKing.Jikan;
using PaperMalKing.Jikan.Data.Interfaces;
using PaperMalKing.Jikan.Data.Models;

namespace PaperMalKing.Services
{
	public sealed class MalService
	{
		/// <summary>
		/// Is currently bot checking for updates
		/// </summary>
		public bool Updating { get; private set; }

		/// <summary>
		/// Regex that is used to get Id from urls
		/// </summary>
		private readonly Regex _regex = new Regex(@"(?<=\/)(\d*?)(?=\/)", RegexOptions.Compiled);

		private readonly ConcurrentDictionary<long, DiscordWebhook> _webhooks;

		private readonly BotConfig _config;

		private readonly DiscordClient _client;

		private readonly Timer _timer;

		private readonly JikanClient _jikanClient;

		private readonly string _logName;


		private delegate Task UpdateFoundHandler(ListUpdateEntry update);

		private event UpdateFoundHandler UpdateFound;

		public MalService(BotConfig config, DiscordClient client)
		{
			this._webhooks = new ConcurrentDictionary<long, DiscordWebhook>();
			this._config = config;
			this._client = client;
			client.Ready += this.Client_Ready;
			this.UpdateFound += this.MalService_UpdateFound;
			this._jikanClient = new JikanClient();
			this._logName = this.GetType().Name;
			this._timer = new Timer(async (e) =>
				{
					try
					{
						await this.Timer_Tick();
					}
					catch(Exception ex)
					{
						this._client.DebugLogger.LogMessage(LogLevel.Error, this._logName,
							"Exception occured in Timer_Tick method", DateTime.Now, ex);
					}
				}, null, TimeSpan.FromSeconds(10),
				TimeSpan.FromMinutes(10));
		}

		public void RestartTimer()
		{
			if (!this.Updating)
				this._timer.Change(
					TimeSpan.Zero, TimeSpan.FromMinutes(10));
		}

		public async Task AddUserAsync(DiscordMember member, string username)
		{
			var userId =(long) member.Id;
			using (var db = new DatabaseContext(this._config))
			{
				var user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
				if (user == null) //User is adding himself in the first time
				{
					var animeRssUrl = $"https://myanimelist.net/rss.php?type=rw&u={username}";
					var mangaRssUrl = $"https://myanimelist.net/rss.php?type=rm&u={username}";
					try
					{
						await FeedReader.ReadAsync(animeRssUrl);
						await FeedReader.ReadAsync(mangaRssUrl);
					}
					catch
					{
						throw new Exception("Couldn't read your updates. Maybe your list isn't public");
					}
					var guildId = (long) member.Guild.Id;
					var guilds = new List<GuildUsers> {new GuildUsers {DiscordId = userId, GuildId = guildId}};

					var pmkUser = new PmkUser
						{DiscordId = userId, LastUpdateDate = DateTime.Now.ToUniversalTime(), MalUsername = username, Guilds = guilds};

					db.Users.Add(pmkUser);
					this._client.DebugLogger.LogMessage(LogLevel.Info, this._logName,
						$"Added new user '{username}'({member})", DateTime.Now);
				}
				else // User is already saved in another guilds
				{
					var guildId = (long) member.Guild.Id;
					if (user.Guilds != null && user.Guilds.All(x=>x.GuildId != guildId))
					{
						user.Guilds.Add(new GuildUsers {DiscordId = user.DiscordId, GuildId = guildId});
						db.Update(user);
						this._client.DebugLogger.LogMessage(LogLevel.Info, this._logName,
							$"Added ({member}) in guild '{guildId}'", DateTime.Now);

					}
				}

				var rowChanged = await db.SaveChangesAsync();

				if (rowChanged == 0)
					throw new Exception("Couldn't save in database. Try again later.");
			}
		}

		public async Task AddUserHereAsync(DiscordMember member)
		{
			var userId =(long) member.Id;
			using (var db = new DatabaseContext(this._config))
			{
				var user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
				if (user == null)
					throw new Exception("You must add username in this or other guild first");
				var guildId = (long) member.Guild.Id;
				if (user.Guilds != null && user.Guilds.All(x=>x.GuildId != guildId))
				{
					user.Guilds.Add(new GuildUsers {DiscordId = user.DiscordId, GuildId = guildId});
					db.Update(user);
					this._client.DebugLogger.LogMessage(LogLevel.Info, this._logName,
						$"Added ({member}) in guild '{guildId}'", DateTime.Now);
					var rowChanged = await db.SaveChangesAsync();
				}
			}
		}


		public void RemoveUserEverywhere(DiscordMember member)
		{
			using (var db = new DatabaseContext(this._config))
			{
				var userId = (long) member.Id;
				var user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
				if (user == null)
					return;
				db.Users.Remove(user);
				var rowsChanged = db.SaveChanges();
				if (rowsChanged == 0)
					throw new Exception("Couldn't save changes in database. Try again later");
				this._client.DebugLogger.LogMessage(LogLevel.Info, this._logName,
					$"Sucessfully removed user '{user.MalUsername}'({member}) from all guilds", DateTime.Now);

			}
		}

		public void RemoveUserHere(DiscordMember member)
		{
			using (var db = new DatabaseContext(this._config))
			{
				var userId = (long) member.Id;
				var guildId = (long) member.Guild.Id;
				var user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
				if (user == null)
					return;
				if (user.Guilds.Count == 1)
				{
					db.Users.Remove(user);
				}
				else
				{
					var guild = user.Guilds.FirstOrDefault(x => x.GuildId == guildId);
					if (guild == null)
						return;
					user.Guilds.Remove(guild);
					db.Users.Update(user);
				}
				var rowsChanged = db.SaveChanges();
				if (rowsChanged == 0)
					throw new Exception("Couldn't save changes in database. Try again later");
				this._client.DebugLogger.LogMessage(LogLevel.Info, this._logName,
					$"Sucessfully removed user '{user.MalUsername}'({member}) from {member.Guild}", DateTime.Now);

			}
		}

		public async Task UpdateUserAsync(long userId, string newUsername)
		{
			using (var db = new DatabaseContext(this._config))
			{
				var user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
				if (user == null)
					throw new ArgumentException("User is not in database",nameof(user));
				var oldUsername = user.MalUsername;
				user.MalUsername = newUsername;
				try
				{
					await FeedReader.ReadAsync(user.AnimeRssFeed);
					await FeedReader.ReadAsync(user.MangaRssFeed);
				}
				catch
				{
					throw new Exception("Couldn't read your updates. Maybe your list isn't public");
				}
				db.Users.Update(user);
				var rowChanges = db.SaveChanges();
				if (rowChanges == 0)
					throw new Exception("Couldn't save update in database. Try again later");
				this._client.DebugLogger.LogMessage(LogLevel.Info, this._logName,
					$"Updated user with id'{userId}' from '{oldUsername}' to '{newUsername}'", DateTime.Now);

			}
		}

		public async Task AddWebhookAsync(long guildId, string webhookUrl)
		{
			var webhookUnparsedId = this._regex.Matches(webhookUrl)
			.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Value))
			?.Value;
			if (!long.TryParse(webhookUnparsedId, out long webhookId))
				throw new Exception($"Couldn't parse id from {webhookUrl}");
			var uWebhookId = (ulong) webhookId;
			var index = webhookUrl.LastIndexOf('/');
			var token = webhookUrl.Substring(index + 1);
			var webhook = await this._client.GetWebhookWithTokenAsync(uWebhookId, token);


			using (var db = new DatabaseContext(this._config))
			{

				var guild = db.Guilds.FirstOrDefault(x => x.GuildId == guildId);
				if (guild == null)
				{
					guild = new PmkGuild {GuildId = guildId, WebhookId = webhookId, WebhookToken = token};

					db.Guilds.Add(guild);
				}
				else if (guild.WebhookId == null)
				{
					guild.WebhookId = webhookId;
					guild.WebhookToken = token;
				}
				else
					throw new Exception("Guild with webhook is already in database. Use WebhookUpdate command instead of WebhookAdd");

				db.SaveChanges();

			}

			this._webhooks.TryAdd(guildId, webhook);
			this._client.DebugLogger.LogMessage(LogLevel.Info, this._logName,
				$"Sucessfully added webhook in guild with id '{guildId}'", DateTime.Now);

		}

		public async Task UpdateWebhookAsync(long guildId, string webhookUrl)
		{
			var webhookUnparsedId = this._regex.Matches(webhookUrl)
			.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Value))
			?.Value;
			if (!long.TryParse(webhookUnparsedId, out long webhookId))
				throw new Exception($"Couldn't parse id from {webhookUrl}");
			var uWebhookId = (ulong) webhookId;
			var index = webhookUrl.LastIndexOf('/');
			var token = webhookUrl.Substring(index + 1);
			var webhook = await this._client.GetWebhookWithTokenAsync(uWebhookId, token);

			using (var db = new DatabaseContext(this._config))
			{
				var guild = db.Guilds.FirstOrDefault(x => x.GuildId == guildId);
				if(guild == null)
					throw new Exception("Webhook is not saved in database try to add it instead of updating it");
				guild.WebhookId = webhookId;
				db.Guilds.Update(guild);
				db.SaveChanges();
			}

			this._webhooks[guildId] = webhook;
			this._client.DebugLogger.LogMessage(LogLevel.Info, this._logName,
				$"Sucessfully updated webhook in guild with id '{guildId}'", DateTime.Now);

		}

		public void RemoveWebhook(long guildId)
		{
			using (var db = new DatabaseContext(this._config))
			{
				var guild = db.Guilds.FirstOrDefault(x => x.GuildId == guildId);
				if (guild != null)
				{
					this._webhooks.TryRemove(guildId, out var _);
					if (guild.WebhookId == null) return;
					guild.WebhookId = null;
					guild.WebhookToken = null;
					db.Guilds.Update(guild);
					db.SaveChanges();
				}
			}
			this._client.DebugLogger.LogMessage(LogLevel.Info, this._logName,
				$"Sucessfully removed webhook in guild with id '{guildId}'", DateTime.Now);

		}

		private async Task<IMalEntity> GetMalEntityAsync(EntityType type, FeedItem feedItem, PmkUser pmkUser,UserProfile profile)
		{
			var actionString = feedItem.Description.Split(" - ")[0].ToLower();
			var malUnparsedId = this._regex.Matches(feedItem.Link)
			.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Value))
			?.Value;

			if (!long.TryParse(malUnparsedId, out long malId))
			{
				this._client.DebugLogger.LogMessage(LogLevel.Error, this._logName,
					$"Couldn't parse {malUnparsedId}", DateTime.Now);
				return null;
			}

			if (actionString.Contains("plan to"))
			{
				if (type == EntityType.Anime)
					return await this._jikanClient.GetAnimeAsync(malId);
				return await this._jikanClient.GetMangaAsync(malId);
			}

			var index = feedItem.Title.LastIndexOf(" - ");
			var query = feedItem.Title.Remove(index).Trim();
			await Task.Delay(TimeSpan.FromSeconds(4));
			if (type == EntityType.Anime)
			{
				var userAl = await this._jikanClient.GetUserAnimeListAsync(pmkUser.MalUsername, query);
				if (userAl?.Anime?.Any() != true)
				{
					this._client.DebugLogger.LogMessage(LogLevel.Error, this._logName,
						$"Couldn't load '{query}' from '{pmkUser.MalUsername}'s animelist", DateTime.Now);
					return await this._jikanClient.GetAnimeAsync(malId);
				}
				return userAl.Anime.FirstOrDefault(x => x.MalId == malId);
			}

			var userMl = await this._jikanClient.GetUserMangaList(pmkUser.MalUsername, query);
			if (userMl?.Manga?.Any() != true)
			{
				this._client.DebugLogger.LogMessage(LogLevel.Error, this._logName,
					$"Couldn't load '{query}' from '{pmkUser.MalUsername}'s mangalist", DateTime.Now);
				return await this._jikanClient.GetMangaAsync(malId);
			}
			return userMl.Manga.FirstOrDefault(x => x.MalId == malId);
		}

		private async Task MalService_UpdateFound(ListUpdateEntry update)
		{
			await Task.Yield();
			this._client.DebugLogger.LogMessage(LogLevel.Info, this._logName,
				$"Sending update for {update.Entry.Title} in {update.UserProfile.Username} MAL", DateTime.Now);

			foreach (var guildId in update.User.Guilds.Select(x => x.GuildId))
			{
				var embed = update.CreateEmbed();
				if (this._webhooks.TryGetValue(guildId, out var webhook))
				{
					await webhook.ExecuteAsync(username: this._client.CurrentUser.Username,
						avatar_url: this._client.CurrentUser.AvatarUrl, embeds: new[] {embed}, files: null);
				}
			}
		}

		private async Task Client_Ready(ReadyEventArgs e)
		{
			using (var db = new DatabaseContext(this._config))
			{
				foreach (var guild in db.Guilds)
				{
					try
					{
						if (guild.WebhookId == null)
							continue;
						var webhookId = (ulong) guild.WebhookId.Value;
						var webhook = await this._client.GetWebhookWithTokenAsync(webhookId, guild.WebhookToken);
						this._webhooks.TryAdd(guild.GuildId, webhook);
						e.Client.DebugLogger.LogMessage(LogLevel.Info, this._logName,
							$"Successfully loaded webhook for guild with id '{guild.GuildId}'", DateTime.Now);
					}
					catch(Exception ex)
					{
						e.Client.DebugLogger.LogMessage(LogLevel.Critical, this._logName,
							$"Webhook wasn't loaded succesfully in guild with id '{guild.GuildId}'", DateTime.Now, ex);
					}
				}
			}

			e.Client.DebugLogger.LogMessage(LogLevel.Info, this._logName, "Loaded webhooks for all guilds", DateTime.Now);

			this._client.Ready -= this.Client_Ready;
		}

		// Cleaned up a bit should look better now
		private  async Task Timer_Tick()
		{
			this.Updating = true;
			this._client.DebugLogger.LogMessage(LogLevel.Info, this._logName, "Starting checking for updates",
				DateTime.Now);
			PmkUser[] users;
			using (var db = new DatabaseContext(this._config))
			{
				users =	db.Users.Include(ug=>ug.Guilds).ThenInclude(g=>g.Guild).ToArray();
			}

			foreach (var user in users)
			{
				this._client.DebugLogger.LogMessage(LogLevel.Info, this._logName,
					$"Starting to checking updates for {user.MalUsername}", DateTime.Now);
				Feed animeFeed;
				Feed mangaFeed;
				DateTime? readFeedDate;
				try
				{
					animeFeed = await FeedReader.ReadAsync(user.AnimeRssFeed);
					mangaFeed = await FeedReader.ReadAsync(user.MangaRssFeed);
					readFeedDate = DateTime.Now;
				}
				catch(Exception e)
				{
					this._client.DebugLogger.LogMessage(LogLevel.Error, this._logName,
						$"Couldn't load rss feed for {user.MalUsername}", DateTime.Now, e);
					continue;
				}

				var newAnimeItems = animeFeed.Items.Where(x =>
					DateTime.Compare((x.PublishingDate ?? DateTime.MinValue).ToUniversalTime(), user.LastUpdateDate) >
					0);
				var newMangaItems = mangaFeed.Items.Where(x =>
					DateTime.Compare((x.PublishingDate ?? DateTime.MinValue).ToUniversalTime(), user.LastUpdateDate) >
					0);

				if(!newMangaItems.Any() && !newAnimeItems.Any())
					continue;

				await Task.Delay(TimeSpan.FromSeconds(4));
				var malUser = await this._jikanClient.GetUserProfileAsync(user.MalUsername);
				if (malUser == null)
				{
					this._client.DebugLogger.LogMessage(LogLevel.Error, this._logName,
						$"Couldn't load MyAnimeList user from username '{user.MalUsername}' (DiscordId '{user.DiscordId}'",
						DateTime.Now);
					continue;
				}

				var updateItems = new List<(FeedItem, EntityType)>();
				updateItems.AddRange(newAnimeItems.Select(x => (x, EntityType.Anime)));
				updateItems.AddRange(newMangaItems.Select(x => (x, EntityType.Manga)));
				updateItems.Sort((x, y) => DateTime.Compare(x.Item1.PublishingDate ?? DateTime.MinValue,
					y.Item1.PublishingDate ?? DateTime.MinValue));

				foreach (var updateItem in updateItems)
				{
					var malEntity = await this.GetMalEntityAsync(updateItem.Item2, updateItem.Item1, user, malUser);
					if (malEntity != null)
					{
						var listUpdateEntry = new ListUpdateEntry(malUser,user, malEntity, updateItem.Item1.Description,
							updateItem.Item1.PublishingDate);
						await this.UpdateFound?.Invoke(listUpdateEntry);
					}
				}

				user.LastUpdateDate = readFeedDate.Value.ToUniversalTime();
				using (var db = new DatabaseContext(this._config))
				{
					db.Users.Update(user);
					await db.SaveChangesAsync();
				}
			}

			this.Updating = false;
			this._client.DebugLogger.LogMessage(LogLevel.Info, this._logName, "Ended checking for updates",
				DateTime.Now);
		}
	}
}
