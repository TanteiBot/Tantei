/*
 * Cleaned up a bit should look nice now
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using CodeHollow.FeedReader;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using PaperMalKing.Data;
using PaperMalKing.Jikan;
using PaperMalKing.Jikan.Data.Interfaces;
using PaperMalKing.Jikan.Data.Models;

namespace PaperMalKing.Services
{
	public sealed class MalService
	{
		/// <summary>
		/// User's which updates bot is currently tracking
		/// </summary>
		public Dictionary<long, PmkUser> Users { get; set; }

		/// <summary>
		/// Is currently bot checking for updates
		/// </summary>
		public bool Updating { get; private set; }

		/// <summary>
		/// Channel where all updates will be posted
		/// </summary>
		private DiscordChannel Channel { get; set; }

		/// <summary>
		/// Regex that is used to get Mal Id from RSS feed item
		/// </summary>
		private readonly Regex _regex = new Regex(@"(?<=\/)(\d*?)(?=\/)", RegexOptions.Compiled);

		private readonly BotConfig _config;

		private readonly DiscordClient _client;

		private readonly Timer _timer;

		private readonly JikanClient _jikanClient;

		private readonly string _logName;


		private delegate Task UpdateFoundHandler(ListUpdateEntry update);

		private event UpdateFoundHandler UpdateFound;

		public MalService(BotConfig config, DiscordClient client)
		{
			this.Users = new Dictionary<long, PmkUser>();
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

		public async Task AddUserAsync(DiscordUser user, string username)
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

			var userId =(long) user.Id;
			var pmkUser = new PmkUser {DiscordId = userId, LastUpdateDate = DateTime.Now.ToUniversalTime(), MalUsername = username};
			if (this.Users.ContainsKey(userId))
			{
				throw new Exception("User is already saved");
			}
			using (var db = new DatabaseContext(this._config))
			{
				db.Users.Add(pmkUser);
				var rowChanged = await db.SaveChangesAsync();

				if (rowChanged == 0)
					throw new Exception("Couldn't save in database. Try again later.");
			}
			this.Users.Add(userId, pmkUser);
		}

		public void RemoveUser(long userId)
		{
			using (var db = new DatabaseContext(this._config))
			{
				var user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
				if (user == null)
					throw new ArgumentException("Such user doesn't exist in database",nameof(user));
				db.Users.Remove(user);
				var rowsChanges = db.SaveChanges();
				if (rowsChanges == 0)
					throw new Exception("Couldn't save changes in database. Try again later");

			}
			this.Users.Remove(userId);
		}

		public async Task UpdateAsync(long userId, string newUsername)
		{
			using (var db = new DatabaseContext(this._config))
			{
				var user = db.Users.FirstOrDefault(x => x.DiscordId == userId);
				if (user == null)
					throw new ArgumentException("User is not in database",nameof(user));
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

				this.Users[userId] = user;
			}
		}
		private async Task<IMalEntity> GetMalEntityAsync(EntityType type, FeedItem feedItem, PmkUser pmkUser,UserProfile profile)
		{
			var actionString = feedItem.Description.Split(" - ")[0].ToLower();
			var malUnparsedId = this._regex.Matches(feedItem.Link)
			.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Value)).Value;

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
			this._client.DebugLogger.LogMessage(LogLevel.Info,this._logName, $"Sending update for {update.Entry.Title} in {update.User.Username} MAL",DateTime.Now);

			await this.Channel.SendMessageAsync(embed: update.CreateEmbed());
		}

		private async Task Client_Ready(ReadyEventArgs e)
		{
			try
			{
				this.Channel = await e.Client.GetChannelAsync(this._config.Discord.ChannelId);

			}
			catch(Exception ex)
			{
				e.Client.DebugLogger.LogMessage(LogLevel.Critical, this._logName,
					"Channel wasn't loaded  succesfully check your channel id", DateTime.Now, ex);
				return;
			}

			e.Client.DebugLogger.LogMessage(LogLevel.Info, this._logName, "Loaded channel successfully", DateTime.Now);

			using (var db = new DatabaseContext(this._config))
			{
				foreach (var user in db.Users)
				{
					this.Users.Add(user.DiscordId,user);
				}
			}

			this._client.Ready -= this.Client_Ready;
		}

		// Cleaned up a bit should look better now
		private  async Task Timer_Tick()
		{
			this.Updating = true;
			this._client.DebugLogger.LogMessage(LogLevel.Info, this._logName, "Starting checking for updates",
				DateTime.Now);
			foreach (var user in this.Users.Values)
			{
				this._client.DebugLogger.LogMessage(LogLevel.Info, this._logName, $"Starting to checking updates for {user.MalUsername}",DateTime.Now);
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
					DateTime.Compare((x.PublishingDate ?? DateTime.MinValue).ToUniversalTime(), user.LastUpdateDate) > 0).ToArray();
				var newMangaItems = mangaFeed.Items.Where(x =>
					DateTime.Compare((x.PublishingDate ?? DateTime.MinValue).ToUniversalTime(), user.LastUpdateDate) > 0).ToArray();

				if(!newMangaItems.Any() && !newAnimeItems.Any())
					continue;

				await Task.Delay(TimeSpan.FromSeconds(4));
				var malUser = await this._jikanClient.GetUserProfileAsync(user.MalUsername);
				if (malUser == null)
					throw new Exception(
						$"Couldn't load MyAnimeList user from username '{user.MalUsername}' (DiscordId '{user.DiscordId}'");
				var newItems = newAnimeItems.Concat(newMangaItems);
				foreach (var animeItem in newAnimeItems)
				{
					var malEntity = await this.GetMalEntityAsync(EntityType.Anime, animeItem, user, malUser);
					if (malEntity != null)
					{
						var listUpdateEntry = new ListUpdateEntry(malUser, malEntity, animeItem.Description,
							animeItem.PublishingDate);
						await this.UpdateFound?.Invoke(listUpdateEntry);
					}
				}
				foreach (var mangaItem in newMangaItems)
				{
					var malEntity = await this.GetMalEntityAsync(EntityType.Manga, mangaItem, user, malUser);
					if (malEntity != null)
					{
						var listUpdateEntry = new ListUpdateEntry(malUser, malEntity, mangaItem.Description,
							mangaItem.PublishingDate);
						await this.UpdateFound?.Invoke(listUpdateEntry);
					}
				}

				user.LastUpdateDate = (readFeedDate ?? DateTime.Now).ToUniversalTime();
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
