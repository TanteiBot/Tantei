using System;
using System.Collections.Generic;
using System.Linq;
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
		public override event UpdateFoundEventHandler? UpdateFoundEvent;

		/// <inheritdoc />
		protected override async Task CheckForUpdatesAsync(CancellationToken cancellationToken)
		{
			this.Logger.LogInformation("Starting to check for updates");
			using var scope = this._serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();

			await foreach (var dbUser in db.ShikiUsers.Include(u => u.DiscordUser).ThenInclude(du => du.Guilds).Include(u => u.Favourites)
										   .Where(u => u.DiscordUser.Guilds.Any()).AsAsyncEnumerable().WithCancellation(cancellationToken))
			{
				var totalUpdates = new List<DiscordEmbedBuilder>();
				var historyUpdates = await this._client.GetAllUserHistoryAfterEntryAsync(dbUser.Id, dbUser.LastHistoryEntryId, cancellationToken);
				var favs = await this._client.GetUserFavouritesAsync(dbUser.Id, cancellationToken);
				var cfavs = dbUser.Favourites.Select(f => new Favourites.FavouriteEntry
				{
					Id = f.Id,
					Name = f.Name,
					GenericType = f.FavType
				}).ToArray();
				var (addedValues, removedValues) = cfavs.GetDifference(favs.AllFavourites);
				if (!historyUpdates.Any() && !addedValues.Any() && !removedValues.Any())
				{
					this.Logger.LogDebug($"No updates found for {dbUser.Id.ToString()}.");
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
				var user = await this._client.GetUserInfo(dbUser.Id, cancellationToken);
				totalUpdates.AddRange(removedValues.Select(rf => rf.ToDiscordEmbed(user, false)));
				totalUpdates.AddRange(addedValues.Select(af => af.ToDiscordEmbed(user, true)));
				var groupedHistoryEntries = historyUpdates.GroupSimilarHistoryEntries();
				foreach (var group in groupedHistoryEntries)
					totalUpdates.Add(group.ToDiscordEmbed(user));
				var lastHistoryEntry = historyUpdates.MaxBy(h => h.Id).FirstOrDefault();
				if (lastHistoryEntry != null)
					dbUser.LastHistoryEntryId = lastHistoryEntry.Id;

				if (cancellationToken.IsCancellationRequested)
				{
					this.Logger.LogInformation("Stopping requested");
					db.Entry(user).State = EntityState.Unchanged;
					continue;
				}
				
				this.Logger.LogDebug($"Found {"update".ToQuantity(totalUpdates.Count)} for {user.Nickname} ({user.Id.ToString()})");
				totalUpdates.ForEach(deb => deb.AddField("By", Helpers.ToDiscordMention(dbUser.DiscordUserId), true));
				await this.UpdateFoundEvent?.Invoke(new(new ShikiUpdate(totalUpdates), this, dbUser.DiscordUser))!;
				db.ShikiUsers.Update(dbUser);
				await db.SaveChangesAndThrowOnNoneAsync(CancellationToken.None);
			}

			this.Logger.LogInformation("Ending checks for updates");
		}
	}
}