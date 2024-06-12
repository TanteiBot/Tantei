// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
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

		foreach (var dbUser in db.ShikiUsers.TagWith("Query users for update checking").TagWithCallSite().Where(u =>
					 u.DiscordUser.Guilds.Any() && ((u.Features & ShikiUserFeatures.AnimeList) != 0 ||
													(u.Features & ShikiUserFeatures.MangaList) != 0 ||
													(u.Features & ShikiUserFeatures.Favourites) != 0)).OrderBy(_ => EF.Functions.Random()).ToArray())
		{
			if (cancellationToken.IsCancellationRequested)
			{
				break;
			}

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
				var user = await _client.GetUserInfoAsync(dbUser.Id, cancellationToken);

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
																		IReadOnlyList<HistoryMediaRoles> groupedHistoryEntriesWithMediaAndRoles,
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

	private async Task<IReadOnlyList<HistoryMediaRoles>> GroupHistoryEntriesAsync(IReadOnlyList<History> historyUpdates, ShikiUser dbUser, CancellationToken cancellationToken)
	{
		var groupedHistoryEntriesWithMediaAndRoles = new List<HistoryMediaRoles>(historyUpdates.GroupSimilarHistoryEntries().Select(x => new HistoryMediaRoles(x)));

		foreach (var historyMediaRole in groupedHistoryEntriesWithMediaAndRoles.Where(x =>
					 x.HistoryEntries.Exists(historyEntry => historyEntry.Target is not null)))
		{
			var history = historyMediaRole.HistoryEntries.First(x => x.Target is not null);
			if (dbUser.Features.HasFlag(ShikiUserFeatures.Description) || dbUser.Features.HasFlag(ShikiUserFeatures.Studio) ||
				dbUser.Features.HasFlag(ShikiUserFeatures.Publisher) || dbUser.Features.HasFlag(ShikiUserFeatures.Genres))
			{
				historyMediaRole.Media = history.Target!.Type == ListEntryType.Anime
					? await _client.GetMediaAsync<AnimeMedia>(history.Target!.Id, ListEntryType.Anime, cancellationToken)
					: await _client.GetMediaAsync<MangaMedia>(history.Target!.Id, ListEntryType.Manga, cancellationToken);
			}

			if (dbUser.Features.HasFlag(ShikiUserFeatures.Mangaka) || dbUser.Features.HasFlag(ShikiUserFeatures.Director))
			{
				historyMediaRole.Roles = await _client.GetMediaStaffAsync(history.Target!.Id, history.Target.Type, cancellationToken);
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

			if ((dbUser.Features.HasFlag(ShikiUserFeatures.Mangaka) && isManga) || (dbUser.Features.HasFlag(ShikiUserFeatures.Director) && isAnime))
			{
				favouriteMediaRoles.Roles = await _client.GetMediaStaffAsync(
					favouriteMediaRoles.FavouriteEntry.Id,
					isAnime ? ListEntryType.Anime : ListEntryType.Manga,
					cancellationToken);
			}

			if (((isManga || isAnime) &&
				 (dbUser.Features.HasFlag(ShikiUserFeatures.Description) || dbUser.Features.HasFlag(ShikiUserFeatures.Genres))) ||
				(dbUser.Features.HasFlag(ShikiUserFeatures.Publisher) && isManga) || (dbUser.Features.HasFlag(ShikiUserFeatures.Studio) && isAnime))
			{
				favouriteMediaRoles.Media = (isManga, isAnime) switch
				{
					(true, _) => await _client.GetMediaAsync<MangaMedia>(favouriteMediaRoles.FavouriteEntry.Id, ListEntryType.Manga, cancellationToken),
					(_, true) => await _client.GetMediaAsync<AnimeMedia>(favouriteMediaRoles.FavouriteEntry.Id, ListEntryType.Anime, cancellationToken),
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
			return fe => new ShikiFavourite
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