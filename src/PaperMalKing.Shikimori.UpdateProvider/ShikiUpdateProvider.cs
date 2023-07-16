// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

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
using PaperMalKing.Common;
using PaperMalKing.Common.Enums;
using PaperMalKing.Database;
using PaperMalKing.Database.Models.Shikimori;
using PaperMalKing.Shikimori.Wrapper.Abstractions;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models;
using PaperMalKing.Shikimori.Wrapper.Abstractions.Models.Media;
using PaperMalKing.UpdatesProviders.Base;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;

namespace PaperMalKing.Shikimori.UpdateProvider;

internal sealed class ShikiUpdateProvider : BaseUpdateProvider
{
	private readonly IOptions<ShikiOptions> _options;

	private readonly IShikiClient _client;
	private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

	public ShikiUpdateProvider(ILogger<ShikiUpdateProvider> logger, IOptions<ShikiOptions> options, IShikiClient client,
							   IDbContextFactory<DatabaseContext> dbContextFactory) : base(logger,
		TimeSpan.FromMilliseconds(options.Value.DelayBetweenChecksInMilliseconds))
	{
		this._options = options;
		this._client = client;
		this._dbContextFactory = dbContextFactory;
	}

	public override string Name => Constants.NAME;

	public override event UpdateFoundEvent? UpdateFoundEvent;

	protected override async Task CheckForUpdatesAsync(CancellationToken cancellationToken)
	{
		if (this.UpdateFoundEvent is null)
		{
			return;
		}

		using var db = this._dbContextFactory.CreateDbContext();

		foreach (var dbUser in db.ShikiUsers.Where(u =>
					 u.DiscordUser.Guilds.Any() && ((u.Features & ShikiUserFeatures.AnimeList) != 0 ||
												    (u.Features & ShikiUserFeatures.MangaList) != 0 ||
												    (u.Features & ShikiUserFeatures.Favourites) != 0)).OrderBy(_ => EF.Functions.Random()).ToArray())
		{
			if (cancellationToken.IsCancellationRequested)
				break;
			var historyUpdates = await this._client
										   .GetAllUserHistoryAfterEntryAsync(dbUser.Id, dbUser.LastHistoryEntryId, dbUser.Features, cancellationToken)
										   .ConfigureAwait(false);

			var (addedFavourites, removedFavourites, isfavouritesMismatch) = dbUser switch
			{
				_ when dbUser.Features.HasFlag(ShikiUserFeatures.Favourites) => await this.GetFavouritesUpdateAsync(dbUser, db, cancellationToken)
																						  .ConfigureAwait(false),
				_ => (Array.Empty<FavouriteMediaRoles>(), Array.Empty<FavouriteMediaRoles>(), false)
			};
			if (!historyUpdates.Any() && !addedFavourites.Any() && !removedFavourites.Any())
			{
				this.Logger.LogDebug("No updates found for {@Id}", dbUser.Id);
				continue;
			}

			var totalUpdates = new List<DiscordEmbedBuilder>(historyUpdates.Count + addedFavourites.Count + removedFavourites.Count);
			db.Entry(dbUser).Reference(u => u.DiscordUser).Load();
			db.Entry(dbUser.DiscordUser).Collection(du => du.Guilds).Load();
			var user = await this._client.GetUserInfoAsync(dbUser.Id, cancellationToken).ConfigureAwait(false);
			totalUpdates.AddRange(removedFavourites.Select(rf => rf.ToDiscordEmbed(user, false, dbUser.Features)));
			totalUpdates.AddRange(addedFavourites.Select(af => af.ToDiscordEmbed(user, true, dbUser.Features)));

			var groupedHistoryEntriesWithMediaAndRoles = new List<HistoryMediaRoles>(historyUpdates.GroupSimilarHistoryEntries().Select(x =>
				new HistoryMediaRoles()
				{
					HistoryEntries = x
				}));
			if (groupedHistoryEntriesWithMediaAndRoles.Exists(x => x.HistoryEntries.Find(x => x.Target is not null) is not null))
			{
				foreach (var historyMediaRole in groupedHistoryEntriesWithMediaAndRoles.Where(x => x.HistoryEntries.Exists(historyEntry => historyEntry.Target is not null)))
				{
					var history = historyMediaRole.HistoryEntries.First(x => x.Target is not null);
					if (dbUser.Features.HasFlag(ShikiUserFeatures.Description) || dbUser.Features.HasFlag(ShikiUserFeatures.Studio) ||
						dbUser.Features.HasFlag(ShikiUserFeatures.Publisher) || dbUser.Features.HasFlag(ShikiUserFeatures.Genres))
					{
						historyMediaRole.Media = history.Target!.Type == ListEntryType.Anime
							? await this._client.GetMediaAsync<AnimeMedia>(history.Target!.Id, ListEntryType.Anime, cancellationToken)
										.ConfigureAwait(false)
							: await this._client.GetMediaAsync<MangaMedia>(history.Target!.Id, ListEntryType.Manga, cancellationToken)
										.ConfigureAwait(false);
					}

					if (dbUser.Features.HasFlag(ShikiUserFeatures.Mangaka) || dbUser.Features.HasFlag(ShikiUserFeatures.Director))
					{
						historyMediaRole.Roles = await this._client.GetMediaStaffAsync(history.Target!.Id, history.Target.Type, cancellationToken)
														   .ConfigureAwait(false);
					}
				}
			}

			totalUpdates.AddRange(groupedHistoryEntriesWithMediaAndRoles.Select(group => group.ToDiscordEmbed(user, dbUser.Features)));
			if (historyUpdates.Any())
			{
				dbUser.LastHistoryEntryId = historyUpdates.Max(h => h.Id);
			}

			if (cancellationToken.IsCancellationRequested)
			{
				this.Logger.LogInformation("Stopping requested");
				db.Entry(user).State = EntityState.Unchanged;
				continue;
			}

			if (dbUser.Features.HasFlag(ShikiUserFeatures.Mention))
				totalUpdates.ForEach(deb => deb.AddField("By", Helpers.ToDiscordMention(dbUser.DiscordUserId), true));

			if (dbUser.Features.HasFlag(ShikiUserFeatures.Website))
				totalUpdates.ForEach(deb => deb.WithShikiUpdateProviderFooter());

			try
			{
				if (db.SaveChanges() <= 0) throw new Exception("Couldn't save update in DB");
				await this.UpdateFoundEvent.Invoke(new(new BaseUpdate(totalUpdates), this, dbUser.DiscordUser)).ConfigureAwait(false);
				this.Logger.LogDebug("Found {@Count} updates for {@User}", totalUpdates.Count, user);
				if (isfavouritesMismatch)
				{
					dbUser.FavouritesIdHash = Helpers.FavoritesHash(db.ShikiFavourites.Where(x => x.UserId == dbUser.Id).OrderBy(x => x.Id)
																	  .ThenBy(x => x.FavType)
																	  .Select(x => new FavoriteIdType(x.Id, (byte)x.FavType[0])).ToArray());
					db.SaveChanges();
				}
			}
			#pragma warning disable CA1031
			catch (Exception ex)
				#pragma warning restore CA1031
			{
				this.Logger.LogError(ex, "Error happened while sending update or saving changes to DB");
				throw;
			}
		}
	}

	private async Task<(IReadOnlyList<FavouriteMediaRoles> addedValues, IReadOnlyList<FavouriteMediaRoles> removedValues, bool isFavouritesMismatch)>
		GetFavouritesUpdateAsync(ShikiUser dbUser, DatabaseContext db, CancellationToken cancellationToken)
	{
		async Task FillMediaAndRoles(FavouriteMediaRoles favouriteMediaRoles)
		{
			var (isManga, isAnime) = favouriteMediaRoles switch
			{
				_ when favouriteMediaRoles.FavouriteEntry.GenericType!.Contains("manga", StringComparison.OrdinalIgnoreCase) => (true, false),
				_ when favouriteMediaRoles.FavouriteEntry.GenericType!.Contains("anime", StringComparison.OrdinalIgnoreCase) => (false, true),
				_                                                                                                            => (false, false)
			};
			if ((dbUser.Features.HasFlag(ShikiUserFeatures.Mangaka) && isManga) || (dbUser.Features.HasFlag(ShikiUserFeatures.Director) && isAnime))
			{
				favouriteMediaRoles.Roles = await this._client.GetMediaStaffAsync(favouriteMediaRoles.FavouriteEntry.Id,
					isAnime ? ListEntryType.Anime : ListEntryType.Manga, cancellationToken).ConfigureAwait(false);
			}

			if (((isManga || isAnime) && (dbUser.Features.HasFlag(ShikiUserFeatures.Description) || dbUser.Features.HasFlag(ShikiUserFeatures.Genres))) ||
			    (dbUser.Features.HasFlag(ShikiUserFeatures.Publisher) && isManga) || (dbUser.Features.HasFlag(ShikiUserFeatures.Studio) && isAnime))
			{
				favouriteMediaRoles.Media = (isManga, isAnime) switch
				{
					(true, _) => await this._client.GetMediaAsync<MangaMedia>(favouriteMediaRoles.FavouriteEntry.Id, ListEntryType.Manga, cancellationToken)
										   .ConfigureAwait(false),
					(_, true) => await this._client.GetMediaAsync<AnimeMedia>(favouriteMediaRoles.FavouriteEntry.Id, ListEntryType.Anime, cancellationToken)
										   .ConfigureAwait(false),
					_ => throw new UnreachableException()
				};
			}
		}

		Func<FavouriteEntry, FavouriteMediaRoles> FavouriteToFavouriteMediaRolesSelector()
		{
			return x => new FavouriteMediaRoles()
			{
				FavouriteEntry = x
			};
		}

		static Func<FavouriteEntry, ShikiFavourite> Selector(ShikiUser shikiUser)
		{
			return fe => new ShikiFavourite
			{
				Id = fe.Id,
				Name = fe.Name,
				FavType = fe.GenericType!,
				User = shikiUser
			};
		}

		var favs = await this._client.GetUserFavouritesAsync(dbUser.Id, cancellationToken).ConfigureAwait(false);
		var isFavouritesMismatch = dbUser.FavouritesIdHash != Helpers.FavoritesHash(favs.AllFavourites.OrderBy(x => x.Id)
																						.ThenBy(x => x.GenericType, StringComparer.Ordinal)
																						.Select(
																							x => new FavoriteIdType(x.Id, (byte)x.GenericType![0]))
																						.ToArray());
		if (!isFavouritesMismatch)
		{
			return (Array.Empty<FavouriteMediaRoles>(), Array.Empty<FavouriteMediaRoles>(), isFavouritesMismatch);
		}

		var dbFavs = db.ShikiFavourites.Where(x => x.UserId == dbUser.Id).OrderBy(x => x.Id).ThenBy(x => x.FavType).Select(f => new FavouriteEntry
		{
			Id = f.Id,
			Name = f.Name,
			GenericType = f.FavType
		}).ToArray();
		if ((favs.AllFavourites.Count == 0 && dbFavs.Length == 0) || favs.AllFavourites.SequenceEqual(dbFavs))
		{
			return (Array.Empty<FavouriteMediaRoles>(), Array.Empty<FavouriteMediaRoles>(), isFavouritesMismatch);
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
			await FillMediaAndRoles(favouriteMediaRoles).ConfigureAwait(false);
		}
		foreach (var favouriteMediaRoles in removedFavourites)
		{
			await FillMediaAndRoles(favouriteMediaRoles).ConfigureAwait(false);
		}

		return (addedFavourites, removedFavourites, isFavouritesMismatch);
	}
}