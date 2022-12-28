// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

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
using PaperMalKing.Database;
using PaperMalKing.Database.Models.Shikimori;
using PaperMalKing.Shikimori.Wrapper;
using PaperMalKing.Shikimori.Wrapper.Models;
using PaperMalKing.UpdatesProviders.Base;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;

namespace PaperMalKing.Shikimori.UpdateProvider;

internal sealed class ShikiUpdateProvider : BaseUpdateProvider
{
	private readonly IOptions<ShikiOptions> _options;

	private readonly ShikiClient _client;
	private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;


	public ShikiUpdateProvider(ILogger<ShikiUpdateProvider> logger, IOptions<ShikiOptions> options, ShikiClient client,
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
												    (u.Features & ShikiUserFeatures.Favourites) != 0)).OrderBy(x => EF.Functions.Random()).ToArray())
		{
			if (cancellationToken.IsCancellationRequested)
				break;
			var totalUpdates = new List<DiscordEmbedBuilder>();
			var historyUpdates = await this._client
										   .GetAllUserHistoryAfterEntryAsync(dbUser.Id, dbUser.LastHistoryEntryId, dbUser.Features, cancellationToken)
										   .ConfigureAwait(false);

			var (addedValues, removedValues, isfavouritesMismatch) = dbUser switch
			{
				_ when (dbUser.Features & ShikiUserFeatures.Favourites) != 0 => await this.GetFavouritesUpdateAsync(dbUser, db, cancellationToken)
																						  .ConfigureAwait(false),
				_ => (Array.Empty<FavouriteEntry>(), Array.Empty<FavouriteEntry>(), false)
			};
			if (!historyUpdates.Any() && !addedValues.Any() && !removedValues.Any())
			{
				this.Logger.LogDebug("No updates found for {@Id}", dbUser.Id);
				dbUser.FavouritesIdHash = Helpers.FavoritesHash(db.ShikiFavourites.Where(x => x.UserId == dbUser.Id).OrderBy(x => x.Id)
																  .Select(x => new FavoriteIdType(x.Id, (byte)x.FavType[0])).ToArray());
				db.SaveChanges();
				continue;
			}

			db.Entry(dbUser).Reference(u => u.DiscordUser).Load();
			db.Entry(dbUser.DiscordUser).Collection(du => du.Guilds).Load();
			var user = await this._client.GetUserInfoAsync(dbUser.Id, cancellationToken).ConfigureAwait(false);
			totalUpdates.AddRange(removedValues.Select(rf => rf.ToDiscordEmbed(user, false, dbUser.Features)));
			totalUpdates.AddRange(addedValues.Select(af => af.ToDiscordEmbed(user, true, dbUser.Features)));
			var groupedHistoryEntries = historyUpdates.GroupSimilarHistoryEntries();
			totalUpdates.AddRange(groupedHistoryEntries.Select(group => group.ToDiscordEmbed(user, dbUser.Features)));
			if (historyUpdates.Any())
			{
				var lastHistoryEntryId = historyUpdates.Max(h => h.Id);
				dbUser.LastHistoryEntryId = lastHistoryEntryId;
			}


			if (cancellationToken.IsCancellationRequested)
			{
				this.Logger.LogInformation("Stopping requested");
				db.Entry(user).State = EntityState.Unchanged;
				continue;
			}

			if ((dbUser.Features & ShikiUserFeatures.Mention) != 0)
				totalUpdates.ForEach(deb => deb.AddField("By", Helpers.ToDiscordMention(dbUser.DiscordUserId), true));

			if ((dbUser.Features & ShikiUserFeatures.Website) != 0)
				totalUpdates.ForEach(deb => deb.WithShikiUpdateProviderFooter());

			using var transaction = db.Database.BeginTransaction();
			try
			{
				db.ShikiUsers.Update(dbUser);
				if (db.SaveChanges() <= 0) throw new Exception("Couldn't save update in DB");
				transaction.Commit();
				await this.UpdateFoundEvent.Invoke(new(new BaseUpdate(totalUpdates), this, dbUser.DiscordUser)).ConfigureAwait(false);
				this.Logger.LogDebug("Found {@Count} updates for {@User}", totalUpdates.Count, user);
				if (isfavouritesMismatch)
				{
					dbUser.FavouritesIdHash = Helpers.FavoritesHash(db.ShikiFavourites.Where(x => x.UserId == dbUser.Id).OrderBy(x => x.Id)
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

	private async Task<(IReadOnlyList<FavouriteEntry> addedValues, IReadOnlyList<FavouriteEntry> removedValues, bool isFavouritesMismatch)>
		GetFavouritesUpdateAsync(ShikiUser dbUser, DatabaseContext db, CancellationToken cancellationToken)
	{
		Func<FavouriteEntry, ShikiFavourite> Selector(ShikiUser shikiUser)
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
		var isFavouritesMismatch = dbUser.FavouritesIdHash !=
								   Helpers.FavoritesHash(favs.AllFavourites.Select(x => new FavoriteIdType(x.Id, (byte)x.GenericType![0])).ToArray());
		if (!isFavouritesMismatch)
		{
			return (Array.Empty<FavouriteEntry>(), Array.Empty<FavouriteEntry>(), isFavouritesMismatch);
		}

		var dbFavs = db.ShikiFavourites.Where(x => x.UserId == dbUser.Id).Select(f => new FavouriteEntry
		{
			Id = f.Id,
			Name = f.Name,
			GenericType = f.FavType
		}).OrderBy(x => x.Id).ToArray();
		if ((!favs.AllFavourites.Any() && !dbFavs.Any()) || favs.AllFavourites.SequenceEqual(dbFavs))
		{
			//Debug.Assert(!isFavouritesMismatch);
			return (Array.Empty<FavouriteEntry>(), Array.Empty<FavouriteEntry>(), isFavouritesMismatch);
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

		return (addedValues, removedValues, isFavouritesMismatch);
	}
}