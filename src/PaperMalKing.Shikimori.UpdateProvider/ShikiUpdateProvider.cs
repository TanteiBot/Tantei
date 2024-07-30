// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

internal sealed class ShikiUpdateProvider : BaseUpdateProvider
{
	private readonly IShikiClient _client;
	private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
	private readonly ShikiAchievementsService _achievementsService;

	public ShikiUpdateProvider(ILogger<ShikiUpdateProvider> logger, IOptions<ShikiOptions> options, IShikiClient client, IDbContextFactory<DatabaseContext> dbContextFactory, ShikiAchievementsService achievementsService)
		: base(logger, TimeSpan.FromMilliseconds(options.Value.DelayBetweenChecksInMilliseconds))
	{
		this._client = client;
		this._dbContextFactory = dbContextFactory;
		this._achievementsService = achievementsService;
	}

	public override string Name => Constants.Name;

	public override event AsyncEventHandler<UpdateFoundEventArgs>? UpdateFoundEvent;

	protected override async Task CheckForUpdatesAsync(CancellationToken cancellationToken)
	{
		if (this.UpdateFoundEvent is null)
		{
			return;
		}

		await using var db = this._dbContextFactory.CreateDbContext();

		foreach (var dbUser in db.ShikiUsers.TagWith("Query users for update checking").TagWithCallSite().Where(u =>
					 u.DiscordUser.Guilds.Any() && ((u.Features & ShikiUserFeatures.AnimeList) != 0 ||
													(u.Features & ShikiUserFeatures.MangaList) != 0 ||
													(u.Features & ShikiUserFeatures.Favourites) != 0)).OrderBy(_ => EF.Functions.Random()).ToArray())
		{
			if (cancellationToken.IsCancellationRequested)
			{
				break;
			}

			var historyUpdates = await this._client.GetAllUserHistoryAfterEntryAsync(dbUser.Id, dbUser.LastHistoryEntryId, dbUser.Features, cancellationToken);

			var (addedFavourites, removedFavourites, isFavouriteMismatch) = dbUser switch
			{
				_ when dbUser.Features.HasFlag(ShikiUserFeatures.Favourites) => await this.GetFavouritesUpdateAsync(dbUser, db, cancellationToken),
				_ => ([], [], false),
			};

			var achievementUpdates = dbUser switch
			{
				_ when dbUser.Features.HasFlag(ShikiUserFeatures.Achievements) => await this.GetAchievementsUpdatesAsync(dbUser, cancellationToken),
				_ => [],
			};

			if (historyUpdates is [] && addedFavourites is [] && removedFavourites is [] && achievementUpdates is [])
			{
				this.Logger.NoUpdatesFound(dbUser.Id);
				continue;
			}

			var totalUpdates = new List<DiscordEmbedBuilder>(historyUpdates.Count + addedFavourites.Count + removedFavourites.Count + achievementUpdates.Count);
			db.Entry(dbUser).Reference(u => u.DiscordUser).Load();
			db.Entry(dbUser.DiscordUser).Collection(du => du.Guilds).Load();

			var user = await this._client.GetUserInfoAsync(dbUser.Id, cancellationToken);
			totalUpdates.AddRange(removedFavourites.Select(rf => rf.ToDiscordEmbed(user, added: false, dbUser)));
			totalUpdates.AddRange(addedFavourites.Select(af => af.ToDiscordEmbed(user, added: true, dbUser)));
			totalUpdates.AddRange(achievementUpdates.Select(au => au.ToDiscordEmbed(user, dbUser.Features)));
			var groupedHistoryEntriesWithMediaAndRoles = new List<HistoryMediaRoles>(historyUpdates.GroupSimilarHistoryEntries().Select(x =>
				new HistoryMediaRoles
				{
					HistoryEntries = x,
				}));
			if (groupedHistoryEntriesWithMediaAndRoles.Exists(x => x.HistoryEntries.Find(y => y.Target is not null) is not null))
			{
				foreach (var historyMediaRole in groupedHistoryEntriesWithMediaAndRoles.Where(x =>
							 x.HistoryEntries.Exists(historyEntry => historyEntry.Target is not null)))
				{
					var history = historyMediaRole.HistoryEntries.First(x => x.Target is not null);
					if (dbUser.Features.HasFlag(ShikiUserFeatures.Description) || dbUser.Features.HasFlag(ShikiUserFeatures.Studio) ||
						dbUser.Features.HasFlag(ShikiUserFeatures.Publisher) || dbUser.Features.HasFlag(ShikiUserFeatures.Genres))
					{
						historyMediaRole.Media = history.Target!.Type == ListEntryType.Anime
							? await this._client.GetMediaAsync<AnimeMedia>(history.Target!.Id, ListEntryType.Anime, cancellationToken)
							: await this._client.GetMediaAsync<MangaMedia>(history.Target!.Id, ListEntryType.Manga, cancellationToken);
					}

					if (dbUser.Features.HasFlag(ShikiUserFeatures.Mangaka) || dbUser.Features.HasFlag(ShikiUserFeatures.Director))
					{
						historyMediaRole.Roles = await this._client.GetMediaStaffAsync(history.Target!.Id, history.Target.Type, cancellationToken)
														   ;
					}
				}
			}

			totalUpdates.AddRange(groupedHistoryEntriesWithMediaAndRoles.Select(group => group.ToDiscordEmbed(user, dbUser)));
			if (historyUpdates is not [])
			{
				dbUser.LastHistoryEntryId = historyUpdates.Max(h => h.Id);
			}

			if (dbUser.Features.HasFlag(ShikiUserFeatures.Mention))
			{
				totalUpdates.ForEach(deb => deb.AddField("By", DiscordHelpers.ToDiscordMention(dbUser.DiscordUserId), inline: true));
			}

			if (dbUser.Features.HasFlag(ShikiUserFeatures.Website))
			{
				totalUpdates.ForEach(deb => deb.WithShikiUpdateProviderFooter());
			}

			if (cancellationToken.IsCancellationRequested)
			{
				this.Logger.CancellationRequested();
				db.Entry(user).State = EntityState.Unchanged;
				continue;
			}

			try
			{
				if (db.SaveChanges() <= 0)
				{
					throw new NoChangesSavedException();
				}

				await this.UpdateFoundEvent.InvokeAsync(this, new(new BaseUpdate(totalUpdates), this, dbUser.DiscordUser));
				this.Logger.FoundUpdatesForUser(totalUpdates.Count, user.Nickname);
				if (isFavouriteMismatch)
				{
					dbUser.FavouritesIdHash = HashHelpers.FavoritesHash(db.ShikiFavourites.Where(x => x.UserId == dbUser.Id).OrderBy(x => x.Id)
																		  .ThenBy(x => x.FavType)
																		  .Select(x => new FavoriteIdType(x.Id, (byte)x.FavType[0])).ToArray());
					db.SaveChanges();
				}
			}
			catch (Exception ex)
			{
				this.Logger.ErrorHappenedWhileSendingUpdateOrSavingToDb(ex);
				throw;
			}
		}
	}

	private async Task<(IReadOnlyList<FavouriteMediaRoles> AddedValues, IReadOnlyList<FavouriteMediaRoles> RemovedValues, bool IsFavouritesMismatch)>
		GetFavouritesUpdateAsync(ShikiUser dbUser, DatabaseContext db, CancellationToken cancellationToken)
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
				favouriteMediaRoles.Roles = await this._client.GetMediaStaffAsync(
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
					(true, _) => await this._client
										   .GetMediaAsync<MangaMedia>(favouriteMediaRoles.FavouriteEntry.Id, ListEntryType.Manga, cancellationToken),
					(_, true) => await this._client
										   .GetMediaAsync<AnimeMedia>(favouriteMediaRoles.FavouriteEntry.Id, ListEntryType.Anime, cancellationToken),
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

		var favs = await this._client.GetUserFavouritesAsync(dbUser.Id, cancellationToken);
		var isFavouritesMismatch = !string.Equals(
												dbUser.FavouritesIdHash,
												HashHelpers.FavoritesHash(favs.AllFavourites.OrderBy(x => x.Id)
																			  .ThenBy(x => x.GenericType, StringComparer.Ordinal)
																			  .Select(
																				  x => new FavoriteIdType(x.Id, (byte)x.GenericType![0]))
																			  .ToArray()),
												StringComparison.Ordinal);
		if (!isFavouritesMismatch)
		{
			return ([], [], isFavouritesMismatch);
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
			return ([], [], isFavouritesMismatch);
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

		return (addedFavourites, removedFavourites, isFavouritesMismatch);
	}

	private async Task<IReadOnlyList<ShikiAchievement>> GetAchievementsUpdatesAsync(ShikiUser dbUser, CancellationToken cancellationToken)
	{
		var achievements = await this._client.GetUserAchievementsAsync(dbUser.Id, cancellationToken);
		var result = new List<ShikiAchievement>();
		foreach (var (id, level) in achievements)
		{
			var achievementInfo = this._achievementsService.GetAchievementOrNull(id, level);
			if (achievementInfo is null)
			{
				continue;
			}

			var acs = dbUser.Achievements.Find(x => string.Equals(x.NekoId, id, StringComparison.Ordinal));
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
		}

		return result;
	}
}