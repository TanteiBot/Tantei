// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2025 N0D4N

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Threading;
using PaperMalKing.Common;
using PaperMalKing.Common.Enums;
using PaperMalKing.Database;
using PaperMalKing.Database.Models.Shikimori;
using PaperMalKing.Shikimori.UpdateProvider.Achievements;
using PaperMalKing.Shikimori.Wrapper.Abstractions;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Media;
using PaperMalKing.UpdatesProviders.Base;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;

namespace PaperMalKing.Shikimori.UpdateProvider;

internal sealed class ShikiUpdateProvider(ILogger<ShikiUpdateProvider> logger, IOptionsMonitor<ShikiOptions> _options,
										  IShikiClient _client, IDbContextFactory<DatabaseContext> _dbContextFactory,
										  ShikiAchievementsService _achievementsService)
	: BaseUpdateProvider(logger)
{
	protected override TimeSpan DelayBetweenTimerFires => TimeSpan.FromMilliseconds(_options.CurrentValue.DelayBetweenChecksInMilliseconds);

	public override string Name => Constants.Name;

	public override event AsyncEventHandler<UpdateFoundEventArgs>? UpdateFoundEvent;

	[SuppressMessage("Roslynator", "RCS1261:Resource can be disposed asynchronously", Justification = "Sqlite does not support async")]
	protected override async Task CheckForUpdatesAsync(CancellationToken cancellationToken)
	{
		if (this.UpdateFoundEvent is null)
		{
			return;
		}

		using var db = _dbContextFactory.CreateDbContext();

		foreach (var dbUser in db.ShikiUsersForChecking)
		{
			if (cancellationToken.IsCancellationRequested)
			{
				break;
			}

			using var scope = logger.CheckingForUsersUpdatesScope(dbUser.Id);
			logger.StartingToCheckUpdatesFor(dbUser.Id);

			var historyUpdates = await _client.GetAllUserHistoryAfterEntryAsync(dbUser.Id, dbUser.LastHistoryEntryId, dbUser.Features, cancellationToken);

			var favs = await _client.GetUserFavouritesAsync(dbUser.Id, cancellationToken);
			var isFavouritesMismatch = !dbUser.FavouritesIdHash.Equals(HashHelpers.FavoritesHash(favs.AllFavourites.ToFavoriteIdType()), StringComparison.Ordinal);

			var achievementUpdates = dbUser.Features.HasFlag(ShikiUserFeatures.Achievements) ? await this.GetAchievementsUpdatesAsync(dbUser, cancellationToken) : [];

			var groupedHistoryEntriesWithMediaAndRoles =
				historyUpdates is not [] ? await this.GroupHistoryEntriesAsync(historyUpdates, dbUser, cancellationToken) : [];

			if (historyUpdates is not [] || isFavouritesMismatch || achievementUpdates is not [])
			{
				db.Entry(dbUser).Reference(u => u.DiscordUser).Load();
				db.Entry(dbUser.DiscordUser).Collection(du => du.Guilds).Load();
				var user = await _client.GetUserByIdAsync(dbUser.Id, cancellationToken);

				await this.UpdateFoundEvent.InvokeAsync(this,
					new(new BaseUpdate(this.GetUpdatesAsync(user, dbUser, db, favs, isFavouritesMismatch, groupedHistoryEntriesWithMediaAndRoles, achievementUpdates, cancellationToken)), dbUser.DiscordUser));
			}
			else
			{
				this.Logger.NoUpdatesFound(dbUser.Id);
			}
		}
	}

	private async IAsyncEnumerable<DiscordEmbedBuilder> GetUpdatesAsync(UserInfo user,
																		ShikiUser dbUser,
																		DatabaseContext db,
																		Favourites favs,
																		bool isFavouritesMismatch,
																		IReadOnlyList<HistoryMedia> groupedHistoryEntriesWithMediaAndRoles,
																		IReadOnlyList<ShikiAchievement> achievementUpdates,
																		[EnumeratorCancellation] CancellationToken cancellationToken)
	{
		static DiscordEmbedBuilder FormatEmbed(ShikiUser dbUser, DiscordEmbedBuilder deb)
		{
			if (dbUser.Features.HasFlag(ShikiUserFeatures.Mention))
			{
				deb.AddField("By", DiscordHelpers.ToDiscordMention(dbUser.DiscordUserId), inline: true);
			}

			if (dbUser.Features.HasFlag(ShikiUserFeatures.Website))
			{
				deb.WithShikiUpdateProviderFooter();
			}

			return deb;
		}

		var updatesCount = 0;

		if (achievementUpdates is not [])
		{
			foreach (var achievementUpdate in achievementUpdates)
			{
				yield return FormatEmbed(dbUser, achievementUpdate.ToDiscordEmbed(user, dbUser.Features));

				updatesCount++;
			}

			await db.SaveChangesAndThrowOnNoneAsync(cancellationToken);
		}

		var (addedFavourites, removedFavourites) = dbUser.Features.HasFlag(ShikiUserFeatures.Favourites) && isFavouritesMismatch ?
			await this.GetFavouritesUpdateAsync(favs, dbUser, db, cancellationToken) : ([], []);

		if (addedFavourites is not [] || removedFavourites is not [])
		{
			foreach (var deb in addedFavourites.Select(af => af.ToDiscordEmbed(user, added: true, dbUser)).Concat(removedFavourites.Select(rf => rf.ToDiscordEmbed(user, added: false, dbUser))))
			{
				yield return FormatEmbed(dbUser, deb);

				updatesCount++;
			}

			dbUser.FavouritesIdHash = HashHelpers.FavoritesHash(db.ShikiFavourites.Where(x => x.UserId == dbUser.Id).OrderBy(x => x.Id)
																				  .ThenBy(x => x.FavType)
																				  .Select(x => new FavoriteIdType(x.Id, (byte)x.FavType[0])).ToArray());
			db.SaveChanges();
		}

		if (groupedHistoryEntriesWithMediaAndRoles is not [])
		{
			var resultingId = groupedHistoryEntriesWithMediaAndRoles.Max(gh => gh.MaxId);

			for (var i = 0; i < groupedHistoryEntriesWithMediaAndRoles.Count; i++)
			{
				var groupedHistory = groupedHistoryEntriesWithMediaAndRoles[i];
				yield return FormatEmbed(dbUser, groupedHistory.ToDiscordEmbed(user, dbUser));

				if (i == 0 || dbUser.LastHistoryEntryId < groupedHistory.MinId)
				{
					dbUser.LastHistoryEntryId = groupedHistory.MinId;
					await db.SaveChangesAndThrowOnNoneAsync(cancellationToken);
				}
			}

			dbUser.LastHistoryEntryId = resultingId;
			db.SaveChanges();
		}

		this.Logger.FoundUpdatesForUser(updatesCount, user.Nickname);
	}

	private async Task<IReadOnlyList<HistoryMedia>> GroupHistoryEntriesAsync(IReadOnlyList<History> historyUpdates, ShikiUser dbUser, CancellationToken cancellationToken)
	{
		var groupedHistoryEntriesWithMediaAndRoles = new List<HistoryMedia>(historyUpdates.GroupSimilarHistoryEntries().Select(x => new HistoryMedia(x)));

		var options = (RequestOptions)dbUser.Features;

		foreach (var historyMediaRole in groupedHistoryEntriesWithMediaAndRoles.Where(x =>
					 x.HistoryEntries.Exists(historyEntry => historyEntry.Target is not null)))
		{
			var history = historyMediaRole.HistoryEntries.First(x => x.Target is not null);
			var isAnime = history.Target!.Type == ListEntryType.Anime;
			var isManga = !isAnime;

			if (dbUser.Features.HasAnyFlag(ShikiUserFeatures.Description, ShikiUserFeatures.Genres) ||
				(isAnime && dbUser.Features.HasAnyFlag(ShikiUserFeatures.Studio, ShikiUserFeatures.Director))
				|| (isManga && dbUser.Features.HasAnyFlag(ShikiUserFeatures.Publisher, ShikiUserFeatures.Mangaka)))
			{
				historyMediaRole.Media = isAnime
					? await _client.GetMediaAsync<AnimeMedia>(history.Target!.Id, ListEntryType.Anime, options, cancellationToken)
					: await _client.GetMediaAsync<MangaMedia>(history.Target!.Id, ListEntryType.Manga, options, cancellationToken);
			}
		}

		return groupedHistoryEntriesWithMediaAndRoles;
	}

	private async Task<(IReadOnlyList<FavouriteMediaRoles> AddedValues, IReadOnlyList<FavouriteMediaRoles> RemovedValues)>
		GetFavouritesUpdateAsync(Favourites favs, ShikiUser dbUser, DatabaseContext db, CancellationToken cancellationToken)
	{
		async Task FillMediaAndRolesAsync(FavouriteMediaRoles favouriteMediaRoles)
		{
			var (isManga, isAnime) = favouriteMediaRoles switch
			{
				_ when favouriteMediaRoles.FavouriteEntry.GenericType!.Contains("manga", StringComparison.OrdinalIgnoreCase) => (true, false),
				_ when favouriteMediaRoles.FavouriteEntry.GenericType!.Contains("anime", StringComparison.OrdinalIgnoreCase) => (false, true),
				_ => (false, false),
			};

			var requestOptions = (RequestOptions)dbUser.Features;

			if (dbUser.Features.HasAnyFlag(ShikiUserFeatures.Description, ShikiUserFeatures.Genres) ||
				(isAnime && dbUser.Features.HasAnyFlag(ShikiUserFeatures.Studio, ShikiUserFeatures.Director))
				|| (isManga && dbUser.Features.HasAnyFlag(ShikiUserFeatures.Publisher, ShikiUserFeatures.Mangaka)))
			{
				favouriteMediaRoles.Media = (isManga, isAnime) switch
				{
					(true, _) => await _client.GetMediaAsync<MangaMedia>(favouriteMediaRoles.FavouriteEntry.Id, ListEntryType.Manga, requestOptions, cancellationToken),
					(_, true) => await _client.GetMediaAsync<AnimeMedia>(favouriteMediaRoles.FavouriteEntry.Id, ListEntryType.Anime, requestOptions, cancellationToken),
					_ => throw new UnreachableException(),
				};
			}
		}

		Func<FavouriteEntry, FavouriteMediaRoles> FavouriteToFavouriteMediaRolesSelector()
		{
			return x => new()
			{
				FavouriteEntry = x,
			};
		}

		static Func<FavouriteEntry, ShikiFavourite> Selector(ShikiUser shikiUser)
		{
			return fe => new()
			{
				Id = fe.Id,
				Name = fe.Name,
				FavType = fe.GenericType!,
				User = shikiUser,
			};
		}

		var dbFavs = db.ShikiFavourites.TagWith("Query favorites info").TagWithCallSite().Where(x => x.UserId == dbUser.Id).OrderBy(x => x.Id)
					   .ThenBy(x => x.FavType).Select(f => new FavouriteEntry
					   {
						   Id = f.Id,
						   Name = f.Name,
						   GenericType = f.FavType,
					   }).ToArray();
		if (favs.AllFavourites.SequenceEqual(dbFavs))
		{
			return ([], []);
		}

		var (addedValues, removedValues) = dbFavs.GetDifference(favs.AllFavourites);
		if (addedValues.Count > 0)
		{
			db.ShikiFavourites.AddRange(addedValues.Select(Selector(dbUser)));
		}

		if (removedValues.Count > 0)
		{
			db.ShikiFavourites.RemoveRange(removedValues.Select(Selector(dbUser)));
		}

		var addedFavourites = addedValues.Select(FavouriteToFavouriteMediaRolesSelector()).ToArray();
		var removedFavourites = removedValues.Select(FavouriteToFavouriteMediaRolesSelector()).ToArray();
		foreach (var favouriteMediaRoles in addedFavourites)
		{
			await FillMediaAndRolesAsync(favouriteMediaRoles);
		}

		foreach (var favouriteMediaRoles in removedFavourites)
		{
			await FillMediaAndRolesAsync(favouriteMediaRoles);
		}

		return (addedFavourites, removedFavourites);
	}

	private async Task<IReadOnlyList<ShikiAchievement>> GetAchievementsUpdatesAsync(ShikiUser dbUser, CancellationToken cancellationToken)
	{
		if (!_achievementsService.IsAnyAchievementInfoAvailable)
		{
			return [];
		}

		var achievements = await _client.GetUserAchievementsAsync(dbUser.Id, cancellationToken);
		var result = new List<ShikiAchievement>();
		foreach (var (id, level) in achievements)
		{
			var achievementInfo = _achievementsService.GetAchievementOrNull(id, level);
			if (achievementInfo is null)
			{
				continue;
			}

			var acs = dbUser.Achievements.Find(x => x.NekoId.Equals(id, StringComparison.Ordinal));
			if (acs is not null && acs.Level < level)
			{
				acs.Level = level;
				result.Add(achievementInfo);
			}
			else if (acs is null)
			{
				dbUser.Achievements.Add(new()
				{
					Level = level,
					NekoId = id,
				});
				result.Add(achievementInfo);
			}
			else
			{
				// Users achievement is of same or higher level, no action needs to be done
			}
		}

		return result;
	}
}