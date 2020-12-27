using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MoreLinq;
using PaperMalKing.Common;
using PaperMalKing.Database;
using PaperMalKing.Database.Models.MyAnimeList;
using PaperMalKing.MyAnimeList.Wrapper;
using PaperMalKing.MyAnimeList.Wrapper.Models;
using PaperMalKing.MyAnimeList.Wrapper.Models.Favorites;
using PaperMalKing.MyAnimeList.Wrapper.Models.List;
using PaperMalKing.MyAnimeList.Wrapper.Models.List.Types;
using PaperMalKing.MyAnimeList.Wrapper.Models.Rss.Types;
using PaperMalKing.UpdatesProviders.Base;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;

namespace PaperMalKing.UpdatesProviders.MyAnimeList
{
	internal sealed class MalUpdateProvider : BaseUpdateProvider
	{
		private readonly MyAnimeListClient _client;
		private readonly IOptions<MalOptions> _options;
		private readonly IServiceProvider _provider;

		/// <inheritdoc />
		public override string Name => Constants.Name;

		public MalUpdateProvider(ILogger<MalUpdateProvider> logger, IOptions<MalOptions> options, MyAnimeListClient client, IServiceProvider provider)
			: base(logger, TimeSpan.FromMilliseconds(options.Value.DelayBetweenChecksInMilliseconds))
		{
			this._options = options;
			this._client = client;
			this._provider = provider;
		}

		/// <inheritdoc />
		public override event UpdateFoundEventHandler? UpdateFoundEvent;

		protected override async Task CheckForUpdatesAsync(CancellationToken cancellationToken)
		{
#region LocalFuncs

			static void DbAnimeUpdateAction(string h, DateTimeOffset dto, MalUser u)
			{
				u.LastAnimeUpdateHash = h;
				u.LastUpdatedAnimeListTimestamp = dto;
			}

			static void DbMangaUpdateAction(string h, DateTimeOffset dto, MalUser u)
			{
				u.LastMangaUpdateHash = h;
				u.LastUpdatedMangaListTimestamp = dto;
			}

			async ValueTask<IReadOnlyList<DiscordEmbedBuilder>> CheckRssListUpdates<TRss, TLe, TL>(
				MalUser dbUser, User user, DateTimeOffset lastUpdateDateTime, Action<string, DateTimeOffset, MalUser> dbUpdateAction,
				CancellationToken ct) where TRss : struct, IRssFeedType where TLe : class, IListEntry where TL : struct, IListType<TLe>
			{
				var rssUpdates = await this._client.GetRecentRssUpdatesAsync<TRss>(user.Username, ct);

				var type = new TRss().Type;
				var updates = rssUpdates.Where(update => update.PublishingDateTimeOffset > lastUpdateDateTime)
										.Select(item => item.ToRecentUpdate(type)).OrderByDescending(u => u.UpdateDateTime).ToArray();

				if (!updates.Any())
					return Array.Empty<DiscordEmbedBuilder>();

				var list = await this._client.GetLatestListUpdatesAsync<TLe, TL>(user.Username, ct);

				foreach (var update in updates)
				{
					var latest = list.FirstOrDefault(entry => entry.Id == update.Id);
					if (latest == null)
						continue;
					else
					{
						dbUpdateAction(latest.GetHash().ToHashString(), update.UpdateDateTime, dbUser);
						break;
					}
				}

				return updates.Select(update => list.First(entry => entry.Id == update.Id).ToDiscordEmbedBuilder(user, update.UpdateDateTime))
							  .ToArray();
			}

			async ValueTask<IReadOnlyList<DiscordEmbedBuilder>> CheckProfileListUpdatesAsync<TLe, TL>(
				MalUser dbUser, User user, int latestUpdateId, DateTimeOffset latestUpdateDateTime,
				Action<string, DateTimeOffset, MalUser> dbUpdateAction, CancellationToken ct)
				where TLe : class, IListEntry where TL : struct, IListType<TLe>
			{
				var listUpdates = await this._client.GetLatestListUpdatesAsync<TLe, TL>(user.Username, ct);
				var lastListUpdate = listUpdates.First(u => u.Id == latestUpdateId);
				dbUpdateAction(lastListUpdate.GetHash().ToHashString(), latestUpdateDateTime, dbUser);

				return new[] {lastListUpdate.ToDiscordEmbedBuilder(user, DateTimeOffset.Now)};
			}

#endregion

			this.Logger.LogInformation("Starting to check for updates");
			using var scope = this._provider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

			await foreach (var dbUser in db.MalUsers.Include(u => u.FavoriteAnimes).Include(u => u.FavoriteMangas).Include(u => u.FavoriteCharacters)
										   .Include(u => u.FavoritePeople).Where(user => user.DiscordUser.Guilds.Any())
										   .AsAsyncEnumerable().WithCancellation(cancellationToken))
			{
				this.Logger.LogDebug($"Starting to check for updates for {dbUser.Username}");
				User? user = null;
				try
				{
					user = await this._client.GetUserAsync(dbUser.Username, cancellationToken);
				}
				catch (HttpRequestException exception) when (exception.StatusCode == HttpStatusCode.NotFound)
				{
					this.Logger.LogError(exception, $"User with username {dbUser.Username} not found");
					dbUser.Username = await this._client.GetUsernameAsync((ulong) dbUser.UserId, cancellationToken);
					db.MalUsers.Update(dbUser);
					await db.SaveChangesAndThrowOnNoneAsync(CancellationToken.None);
					return;
				}
				catch (HttpRequestException exception) when ((int?) exception.StatusCode >= 500)
				{
					this.Logger.LogError(exception, "Mal server encounters some error, skipping current update check");
					return;
				}
				catch (Exception exception)
				{
					this.Logger.LogError(exception, "Encountered unknown error, skipping current update check");
					return;
				}

				if (user is null)
				{
					this.Logger.LogError($"User {dbUser.Username} wasn't initialized due to unknown error");
					return;
				}

				var favoritesUpdates = this.CheckFavoritesUpdates(dbUser, user);

				var animeListUpdates = user.HasPublicAnimeUpdates switch
				{
					true when dbUser.LastAnimeUpdateHash.Substring(" ", true) != user.LatestAnimeUpdate!.Hash.inRssHash => await
						CheckRssListUpdates<AnimeRssFeed, AnimeListEntry, AnimeListType>(dbUser, user, dbUser.LastUpdatedAnimeListTimestamp,
							DbAnimeUpdateAction, cancellationToken),
					true when dbUser.LastAnimeUpdateHash.Substring(" ", false) != user.LatestAnimeUpdate!.Hash.inProfileHash => await
						CheckProfileListUpdatesAsync<AnimeListEntry, AnimeListType>(dbUser, user, user!.LatestAnimeUpdate.Id,
							dbUser.LastUpdatedAnimeListTimestamp, DbAnimeUpdateAction, cancellationToken),
					_ => Array.Empty<DiscordEmbedBuilder>()
				};

				var mangaListUpdates = user.HasPublicMangaUpdates switch
				{
					true when dbUser.LastMangaUpdateHash.Substring(" ", true) != user.LatestMangaUpdate!.Hash.inRssHash => await
						CheckRssListUpdates<MangaRssFeed, MangaListEntry, MangaListType>(dbUser, user, dbUser.LastUpdatedMangaListTimestamp,
							DbMangaUpdateAction, cancellationToken),
					true when dbUser.LastMangaUpdateHash.Substring(" ", false) != user.LatestMangaUpdate!.Hash.inProfileHash => await
						CheckProfileListUpdatesAsync<MangaListEntry, MangaListType>(dbUser, user, user!.LatestMangaUpdate.Id,
							dbUser.LastUpdatedMangaListTimestamp, DbMangaUpdateAction, cancellationToken),
					_ => Array.Empty<DiscordEmbedBuilder>()
				};

				var totalUpdates = favoritesUpdates.Concat(animeListUpdates).Concat(mangaListUpdates)
												   .OrderBy(b => b.Timestamp ?? DateTimeOffset.MinValue).ToArray();
				if (!totalUpdates.Any() || this.UpdateFoundEvent == null)
				{
					db.Entry(dbUser).State = EntityState.Unchanged;
					this.Logger.LogDebug($"Ended checking updates for {user.Username} with no updates found");
					continue;
				}

				await db.Entry(dbUser).Reference(u => u.DiscordUser).LoadAsync(cancellationToken);
				await db.Entry(dbUser.DiscordUser).Collection(du => du.Guilds).LoadAsync(cancellationToken);
				totalUpdates.ForEach(b=> b.WithMalUpdateProviderFooter().AddField("By", Helpers.ToDiscordMention(dbUser.DiscordUser.DiscordUserId), true));
				if (cancellationToken.IsCancellationRequested)
				{
					this.Logger.LogInformation($"Ended checking updates for {user.Username} because it was canceled");
					db.Entry(dbUser).State = EntityState.Unchanged;
					continue;
				}
				await this.UpdateFoundEvent?.Invoke(new(new MalUpdate(totalUpdates), this, dbUser.DiscordUser))!;
				db.Entry(dbUser).State = EntityState.Modified;
				await db.SaveChangesAsync(CancellationToken.None);
				this.Logger.LogDebug($"Ended checking updates for {user.Username} with {"update".ToQuantity(totalUpdates.Length)} found");
			}
		}

		private IReadOnlyList<DiscordEmbedBuilder> CheckFavoritesUpdates(MalUser dbUser, User user)
		{
			IReadOnlyList<DiscordEmbedBuilder> ToDiscordEmbedBuilders<TDbf, TWf>(List<TDbf> original, IReadOnlyList<TWf> resulting, User user,
																				 MalUser dbUser) where TDbf : class, IMalFavorite where TWf : BaseFavorite
			{
				var sor = original.Select(favorite => favorite.Id).OrderBy(i => i).ToArray();
				var sr = resulting.Select(fav => fav.Url.Id).OrderBy(i => i).ToArray();
				if (!original.Any() && !resulting.Any() || sor.SequenceEqual(sr))
				{
					this.Logger.LogTrace($"Didn't find any {typeof(TWf).Name} updates for {dbUser.Username}");
					return Array.Empty<DiscordEmbedBuilder>();
				}
				var cResulting = resulting.Select(favorite => favorite.ToDbFavorite<TDbf>(dbUser)).ToArray();
				var (addedValues, removedValues) = original.GetDifference(cResulting);
				if (!addedValues.Any() && !removedValues.Any())
				{
					this.Logger.LogTrace($"Didn't find any {typeof(TWf).Name} updates for {dbUser.Username}");
					return Array.Empty<DiscordEmbedBuilder>();
				}

				this.Logger.LogTrace(
					$"Found {addedValues.Count.ToString()} new favorites, {removedValues.Count.ToString()} removed favorites of type {typeof(TWf)} of {dbUser.Username}");

				var result = new List<DiscordEmbedBuilder>(addedValues.Count + removedValues.Count);

				for (var i = 0; i < addedValues.Count; i++)
				{
					var fav = cResulting.First(favorite => favorite.Id == addedValues[i].Id);
					var deb = fav.ToDiscordEmbedBuilder(true);
					deb.WithAuthor(user.Username, user.ProfileUrl, user.AvatarUrl);
					result.Add(deb);
				}

				var toRm = new TDbf[removedValues.Count];
				for (var i = 0; i < removedValues.Count; i++)
				{
					var fav = original.First(favorite => favorite.Id == removedValues[i].Id);
					toRm[i] = fav;
					var deb = fav.ToDiscordEmbedBuilder(false);
					deb.WithAuthor(user.Username, user.ProfileUrl, user.AvatarUrl);
					result.Add(deb);
				}

				original.AddRange(addedValues);
				foreach (var t in toRm)
					original.Remove(t);

				return result;
			}

			this.Logger.LogTrace($"Checking favorites updates of {dbUser.Username}");

			var list = new List<DiscordEmbedBuilder>(0);
			list.AddRange(ToDiscordEmbedBuilders(dbUser.FavoriteAnimes, user.Favorites.FavoriteAnime, user, dbUser));
			list.AddRange(ToDiscordEmbedBuilders(dbUser.FavoriteMangas, user.Favorites.FavoriteManga, user, dbUser));
			list.AddRange(ToDiscordEmbedBuilders(dbUser.FavoriteCharacters, user.Favorites.FavoriteCharacters, user, dbUser));
			list.AddRange(ToDiscordEmbedBuilders(dbUser.FavoritePeople, user.Favorites.FavoritePeople, user, dbUser));
			list.Sort((b1, b2) => string.Compare(b1.Title, b2.Title, StringComparison.InvariantCulture));
			return list.OrderBy(deb => deb.Color.HasValue ? deb.Color.Value.Value : DiscordColor.None.Value).ThenBy(deb => deb.Title).ToArray();
		}
	}
}