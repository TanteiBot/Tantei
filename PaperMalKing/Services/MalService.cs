/*
 * Awful code here probably will rewrite when Mal release they public API
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
using JikanDotNet;

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

		private readonly Jikan _jikan;

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
			this._jikan = new Jikan();
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

		// Code here is getting pretty awful, probably won't rewrite it until official MAL API release to public
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
				var malUser = await this._jikan.GetUserProfile(user.MalUsername);
				if (malUser == null)
					throw new Exception(
						$"Couldn't load MyAnimeList user from username '{user.MalUsername}' (DiscordId '{user.DiscordId}'");
				#region GetAnime
				if (newAnimeItems.Any())
				{
					foreach (var animeItem in newAnimeItems)
					{
						var actionString = animeItem.Description.Split(" - ")[0].ToLower();
						var malUnparsedId = this._regex.Matches(animeItem.Link)
						.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Value)).Value;
						if (!long.TryParse(malUnparsedId, out long malId))
						{
							this._client.DebugLogger.LogMessage(LogLevel.Error, this._logName,
								$"Couldn't parse {malUnparsedId}", DateTime.Now);
							continue;
						}

						var userAlExt =
							UserAnimeListExtension.TryParse(actionString.Replace(" ", string.Empty), true,
								out UserAnimeListExtension result)
								? result
								: UserAnimeListExtension.All;
						var status = (ListUpdateEntry.StatusType) userAlExt;
						if (!Enum.IsDefined(typeof(ListUpdateEntry.StatusType), status))
							status = ListUpdateEntry.StatusType.Undefined;

						if (userAlExt == UserAnimeListExtension.PlanToWatch)
						{
							await Task.Delay(TimeSpan.FromSeconds(4));

							var animePage = await this._jikan.GetAnime(malId);
							if (animePage == null)
								throw new Exception($"Couldn't load anime from MalId '{malId}'");
							var updateFromPage = new ListUpdateEntry(malUser, animePage, animeItem.Description, status,animeItem.PublishingDate?.ToUniversalTime());
							await this.UpdateFound?.Invoke(updateFromPage);
							continue;
						}


						var index = animeItem.Title.LastIndexOf(" - ");
						var query = animeItem.Title.Remove(index).Trim();
						await Task.Delay(TimeSpan.FromSeconds(4));
						var userAl = await this._jikan.GetUserAnimeList(user.MalUsername,new UserListAnimeSearchConfig
						{
							Query = query
						});
						if (userAl?.Anime?.Any() != true)
						{
							await Task.Delay(TimeSpan.FromSeconds(4));

							var animePage = await this._jikan.GetAnime(malId);
							if (animePage == null)
								throw new Exception($"Couldn't load anime from MalId '{malId}'");
							var updateFromPage = new ListUpdateEntry(malUser, animePage, animeItem.Description, status,animeItem.PublishingDate?.ToUniversalTime());
							await this.UpdateFound?.Invoke(updateFromPage);
							continue;
						}

						var anime = userAl.Anime.FirstOrDefault(x => x.MalId == malId);
						ListUpdateEntry update;
						if (anime == null)
						{
							await Task.Delay(TimeSpan.FromSeconds(4));
							var animePage = await this._jikan.GetAnime(malId);
							if (animePage == null)
								throw new Exception($"Couldn't load anime from MalId '{malId}'");
							update = new ListUpdateEntry(malUser, animePage, animeItem.Description, status,
								animeItem.PublishingDate?.ToUniversalTime());
						}
						else
							update = new ListUpdateEntry(malUser, anime, animeItem.Description, status,animeItem.PublishingDate?.ToUniversalTime());
						await this.UpdateFound?.Invoke(update);
					}
				}
				#endregion
				#region GetManga
				if (newMangaItems.Any())
				{
					foreach (var mangaItem in newMangaItems)
					{
						var actionString = mangaItem.Description.Split(" - ")[0].ToLower();
						var malUnparsedId = this._regex.Matches(mangaItem.Link)
						.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Value)).Value;
						if (!long.TryParse(malUnparsedId, out long malId))
						{
							this._client.DebugLogger.LogMessage(LogLevel.Error, this._logName,
								$"Couldn't parse {malUnparsedId}", DateTime.Now);
							continue;
						}

						var userMlExt =
							UserMangaListExtension.TryParse(actionString.Replace(" ", string.Empty), true,
								out UserMangaListExtension result)
								? result
								: UserMangaListExtension.All;
						var status = (ListUpdateEntry.StatusType) userMlExt;
						if (!Enum.IsDefined(typeof(ListUpdateEntry.StatusType), status))
							status = ListUpdateEntry.StatusType.Undefined;

						if (userMlExt == UserMangaListExtension.PlanToRead)
						{
							await Task.Delay(TimeSpan.FromSeconds(4));
							var mangaPage = await this._jikan.GetManga(malId);
							if(mangaPage == null)
								throw new Exception($"Couldn't load manga from MalId '{malId}'");
							var updateFromPage = new ListUpdateEntry(malUser, mangaPage, mangaItem.Description, status,mangaItem.PublishingDate?.ToUniversalTime());
							await this.UpdateFound?.Invoke(updateFromPage);
							continue;
						}

						var index = mangaItem.Title.LastIndexOf(" - ");
						var query = mangaItem.Title.Remove(index).Trim();
						await Task.Delay(TimeSpan.FromSeconds(4));
						var userMl = await this._jikan.GetUserMangaList(user.MalUsername,new UserListMangaSearchConfig
						{
							Query = query
						});
						if (userMl?.Manga?.Any() != true)
						{
							await Task.Delay(TimeSpan.FromSeconds(4));
							var mangaPage = await this._jikan.GetManga(malId);
							if(mangaPage == null)
								throw new Exception($"Couldn't load manga from MalId '{malId}'");
							var updateFromPage = new ListUpdateEntry(malUser, mangaPage, mangaItem.Description, status,mangaItem.PublishingDate?.ToUniversalTime());
							await this.UpdateFound?.Invoke(updateFromPage);
							continue;
						}

						var manga = userMl.Manga.FirstOrDefault(x => x.MalId == malId);
						ListUpdateEntry update;
						if (manga == null)
						{
							await Task.Delay(TimeSpan.FromSeconds(4));
							var mangaPage = await this._jikan.GetManga(malId);
							if(mangaPage == null)
								throw new Exception($"Couldn't load manga from MalId '{malId}'");

							update = new ListUpdateEntry(malUser,mangaPage,mangaItem.Description,status,mangaItem.PublishingDate?.ToUniversalTime());
						}
						else
							update = new ListUpdateEntry(malUser, manga, mangaItem.Description, status,mangaItem.PublishingDate?.ToUniversalTime());
						await this.UpdateFound?.Invoke(update);
					}
				}
				#endregion
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
