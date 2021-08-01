#region LICENSE

// PaperMalKing.
// Copyright (C) 2021 N0D4N
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaperMalKing.Common;
using PaperMalKing.Database;
using PaperMalKing.Database.Models.Shikimori;
using PaperMalKing.Shikimori.Wrapper;
using PaperMalKing.Shikimori.Wrapper.Models;
using PaperMalKing.UpdatesProviders.Base;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;

namespace PaperMalKing.Shikimori.UpdateProvider
{
	internal sealed class ShikiUpdateProvider : BaseUpdateProvider
	{
		private readonly IOptions<ShikiOptions> _options;

		private readonly ShikiClient _client;

		private readonly IServiceProvider _serviceProvider;

		/// <inheritdoc />
		public ShikiUpdateProvider(ILogger<ShikiUpdateProvider> logger, IOptions<ShikiOptions> options, ShikiClient client,
								   IServiceProvider serviceProvider) : base(logger,
																			TimeSpan.FromMilliseconds(options.Value.DelayBetweenChecksInMilliseconds))
		{
			this._options = options;
			this._client = client;
			this._serviceProvider = serviceProvider;
		}

		/// <inheritdoc />
		public override string Name => Constants.NAME;

		/// <inheritdoc />
		public override event UpdateFoundEvent? UpdateFoundEvent;

		/// <inheritdoc />
		[SuppressMessage("Microsoft.Design", "CA1031")]
		protected override async Task CheckForUpdatesAsync(CancellationToken cancellationToken)
		{
			using var scope = this._serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

			await foreach (var dbUser in db.ShikiUsers.Include(u => u.DiscordUser).ThenInclude(du => du.Guilds).Include(u => u.Favourites)
										   .Where(u => u.DiscordUser.Guilds.Any()).Where(u => (u.Features & ShikiUserFeatures.AnimeList) != 0 ||
																							  (u.Features & ShikiUserFeatures.MangaList) != 0 ||
																							  (u.Features & ShikiUserFeatures.Favourites) != 0)
										   .AsAsyncEnumerable().WithCancellation(cancellationToken))
			{
				var totalUpdates = new List<DiscordEmbedBuilder>();
				var historyUpdates = await this._client
											   .GetAllUserHistoryAfterEntryAsync(dbUser.Id, dbUser.LastHistoryEntryId, dbUser.Features,
												   cancellationToken).ConfigureAwait(false);
				var favs = dbUser.Features switch
				{
					_ when dbUser.Features.HasFlag(ShikiUserFeatures.Favourites) =>
						await this._client.GetUserFavouritesAsync(dbUser.Id, cancellationToken).ConfigureAwait(false),
					_ => Favourites.Empty
				};
				var cfavs = dbUser.Favourites.Select(f => new FavouriteEntry
				{
					Id = f.Id,
					Name = f.Name,
					GenericType = f.FavType
				}).ToArray();
				var (addedValues, removedValues) = cfavs.GetDifference(favs.AllFavourites);
				if (!historyUpdates.Any() && !addedValues.Any() && !removedValues.Any())
				{
					this.Logger.LogDebug("No updates found for {@Id}", dbUser.Id);
					continue;
				}

				dbUser.Favourites.RemoveAll(f => removedValues.Any(fe => fe.Id == f.Id));
				dbUser.Favourites.AddRange(addedValues.Select(fe => new ShikiFavourite
				{
					Id = fe.Id,
					Name = fe.Name,
					FavType = fe.GenericType!,
					User = dbUser
				}));
				var user = await this._client.GetUserInfo(dbUser.Id, cancellationToken).ConfigureAwait(false);
				totalUpdates.AddRange(removedValues.Select(rf => rf.ToDiscordEmbed(user, false, dbUser.Features)));
				totalUpdates.AddRange(addedValues.Select(af => af.ToDiscordEmbed(user, true, dbUser.Features)));
				var groupedHistoryEntries = historyUpdates.GroupSimilarHistoryEntries();
				foreach (var group in groupedHistoryEntries)
					totalUpdates.Add(group.ToDiscordEmbed(user, dbUser.Features));
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

				if((dbUser.Features & ShikiUserFeatures.Mention) != 0)
					totalUpdates.ForEach(deb => deb.AddField("By", Helpers.ToDiscordMention(dbUser.DiscordUserId), true));

				if ((dbUser.Features & ShikiUserFeatures.Website) != 0)
					totalUpdates.ForEach(deb => deb.WithShikiUpdateProviderFooter());

				await using var transaction = await db.Database.BeginTransactionAsync(CancellationToken.None).ConfigureAwait(false);
				try
				{
					db.ShikiUsers.Update(dbUser);
					if (await db.SaveChangesAsync(CancellationToken.None).ConfigureAwait(false) <= 0) throw new Exception("Couldn't save update in DB");
					await transaction.CommitAsync(CancellationToken.None).ConfigureAwait(false);
					await this.UpdateFoundEvent!.Invoke(new(new BaseUpdate(totalUpdates), this, dbUser.DiscordUser)).ConfigureAwait(false);
					this.Logger.LogDebug("Found {@Count} updates for {@User}", totalUpdates.Count, user);
				}
				catch(Exception ex)
				{
					await transaction.RollbackAsync(CancellationToken.None).ConfigureAwait(false);
					this.Logger.LogError(ex, "Error happened while sending update or saving changes to DB");

				}
			}
		}
	}
}