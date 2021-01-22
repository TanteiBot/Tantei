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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MoreLinq;
using PaperMalKing.AniList.UpdateProvider.CombinedResponses;
using PaperMalKing.AniList.Wrapper;
using PaperMalKing.AniList.Wrapper.Models;
using PaperMalKing.AniList.Wrapper.Models.Enums;
using PaperMalKing.AniList.Wrapper.Models.Interfaces;
using PaperMalKing.Common;
using PaperMalKing.Database;
using PaperMalKing.Database.Models.AniList;
using PaperMalKing.UpdatesProviders.Base;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;
using FavouriteType = PaperMalKing.Database.Models.AniList.FavouriteType;

namespace PaperMalKing.AniList.UpdateProvider
{
    internal sealed class AniListUpdateProvider : BaseUpdateProvider
    {
        private readonly AniListClient _client;
        private readonly IServiceProvider _serviceProvider;

        public AniListUpdateProvider(ILogger<AniListUpdateProvider> logger, IOptions<AniListOptions> options, AniListClient client,
            IServiceProvider serviceProvider) : base(logger, TimeSpan.FromMilliseconds(options.Value.DelayBetweenChecksInMilliseconds))
        {
            this._client = client;
            this._serviceProvider = serviceProvider;
        }

        public override string Name => Constants.NAME;
        public override event UpdateFoundEventHandler? UpdateFoundEvent;

        protected override async Task CheckForUpdatesAsync(CancellationToken cancellationToken)
        {
            using var scope = this._serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            await foreach (var dbUser in db.AniListUsers.Include(au => au.DiscordUser).ThenInclude(du => du.Guilds).Include(au => au.Favourites)
                .Where(du => du.DiscordUser.Guilds.Any()).AsAsyncEnumerable().WithCancellation(cancellationToken))
            {
                this.Logger.LogDebug("Starting to check for updates of {UserId}", dbUser.Id);
                var allUpdates = new List<DiscordEmbedBuilder>();
                var recentUserUpdates = await this._client.GetAllRecentUserUpdatesAsync(dbUser, cancellationToken);
                allUpdates.AddRange(await this.GetFavouritesUpdatesAsync(recentUserUpdates, dbUser, cancellationToken));
                allUpdates.AddRange(recentUserUpdates.Reviews.Where(r => r.CreatedAtTimeStamp > dbUser.LastReviewTimestamp)
                    .Select(r => r.ToDiscordEmbedBuilder(recentUserUpdates.User)));
                foreach (var listActivity in recentUserUpdates.Activities)
                {
                    var mediaListEntry = listActivity.Media.Type == ListType.ANIME
                        ? recentUserUpdates.AnimeList.First(mle => mle.Id == listActivity.Media.Id)
                        : recentUserUpdates.MangaList.First(mle => mle.Id == listActivity.Media.Id);
                    allUpdates.Add(listActivity.ToDiscordEmbedBuilder(mediaListEntry, recentUserUpdates.User));
                }

                if (!allUpdates.Any())
                {
                    this.Logger.LogTrace("No updates found for {Username}", recentUserUpdates.User.Name);
                    db.Entry(dbUser).State = EntityState.Unchanged;
                    continue;
                }

                var lastActivityTimestamp = recentUserUpdates.Activities.MaxBy(a => a.CreatedAtTimestamp).Select(a => a.CreatedAtTimestamp)
                    .FirstOrDefault();
                var lastReviewTimeStamp = recentUserUpdates.Reviews.MaxBy(r => r.CreatedAtTimeStamp).Select(a => a.CreatedAtTimeStamp)
                    .FirstOrDefault();
                if (dbUser.LastActivityTimestamp < lastActivityTimestamp) dbUser.LastActivityTimestamp = lastActivityTimestamp;
                if (dbUser.LastReviewTimestamp < lastReviewTimeStamp) dbUser.LastReviewTimestamp = lastReviewTimeStamp;
                if (cancellationToken.IsCancellationRequested)
                {
                    db.Entry(dbUser).State = EntityState.Unchanged;
                    this.Logger.LogInformation("Cancellation requested. Stopping");
                    continue;
                }

                allUpdates.ForEach(u => u.AddField("By", Helpers.ToDiscordMention(dbUser.DiscordUserId), true));
                allUpdates.Sort((deb1, deb2) => DateTimeOffset.Compare(deb1.Timestamp.GetValueOrDefault(), deb2.Timestamp.GetValueOrDefault()));
                this.UpdateFoundEvent?.Invoke(new(new AniListUpdate(allUpdates), this, dbUser.DiscordUser));
                db.Entry(dbUser).State = EntityState.Modified;
                await db.SaveChangesAndThrowOnNoneAsync(CancellationToken.None);
            }
        }

        private async Task<IReadOnlyList<DiscordEmbedBuilder>> GetFavouritesUpdatesAsync(CombinedRecentUpdatesResponse response, AniListUser user,
            CancellationToken cancellationToken = default)
        {
            static ulong[] GetIds(IReadOnlyList<Favourites.IdentifiableFavourite> collection,
                Func<Favourites.IdentifiableFavourite, bool> predicate)
            {
                return collection.Any(predicate) ? collection.Where(predicate).Select(f => f.Id).ToArray() : Array.Empty<ulong>();
            }

            static void GetFavouritesEmbed<T>(List<DiscordEmbedBuilder> aggregator,
                IReadOnlyList<Favourites.IdentifiableFavourite> addedValues, IReadOnlyList<Favourites.IdentifiableFavourite> removedValues,
                List<T> obtainedValues, Wrapper.Models.Enums.FavouriteType type, User user) where T : class, IIdentifiable, ISiteUrlable
            {
                foreach (var value in obtainedValues)
                {
                    bool? added = null;
                    if (addedValues.Any(f => f.Id == value.Id && f.Type == type))
                        added = true;
                    else if (removedValues.Any(f => f.Id == value.Id && f.Type == type))
                        added = false;

                    if (added.HasValue)
                        aggregator.Add(FavouriteToDiscordEmbedBuilderConverter.Convert(value, user, added.Value));
                }
            }

            var convFavs = user.Favourites.Select(f => new Favourites.IdentifiableFavourite()
            {
                Id = f.Id,
                Type = (Wrapper.Models.Enums.FavouriteType) f.FavouriteType
            }).ToArray();

            var (addedValues, removedValues) = convFavs.GetDifference(response.Favourites);
            if (cancellationToken.IsCancellationRequested || (!addedValues.Any() && !removedValues.Any()))
                return Array.Empty<DiscordEmbedBuilder>();

            user.Favourites.RemoveAll(f => removedValues.Any(rv => rv.Id == f.Id && rv.Type == (Wrapper.Models.Enums.FavouriteType) f.FavouriteType));
            user.Favourites.AddRange(addedValues.Select(av => new AniListFavourite()
                {User = user, UserId = user.Id, Id = av.Id, FavouriteType = (FavouriteType) av.Type}));

            var changedValues = new List<Favourites.IdentifiableFavourite>(addedValues);
            changedValues.AddRange(removedValues);
            var animeIds = GetIds(changedValues, f => f.Type == Wrapper.Models.Enums.FavouriteType.Anime);
            var mangaIds = GetIds(changedValues, f => f.Type == Wrapper.Models.Enums.FavouriteType.Manga);
            var charIds = GetIds(changedValues, f => f.Type == Wrapper.Models.Enums.FavouriteType.Characters);
            var staffIds = GetIds(changedValues, f => f.Type == Wrapper.Models.Enums.FavouriteType.Staff);
            var studioIds = GetIds(changedValues, f => f.Type == Wrapper.Models.Enums.FavouriteType.Studios);
            var hasNextPage = true;
            var combinedResponse = new CombinedFavouritesInfoResponse();
            var results = new List<DiscordEmbedBuilder>(changedValues.Count);
            for (byte page = 1; hasNextPage; page++)
            {
                var favouritesInfo =
                    await this._client.FavouritesInfoAsync(page, animeIds, mangaIds, charIds, staffIds, studioIds, cancellationToken);
                combinedResponse.Add(favouritesInfo);
                hasNextPage = favouritesInfo.HasNextPage;
            }

            GetFavouritesEmbed(results, addedValues, removedValues, combinedResponse.Anime, Wrapper.Models.Enums.FavouriteType.Anime, response.User);
            GetFavouritesEmbed(results, addedValues, removedValues, combinedResponse.Manga, Wrapper.Models.Enums.FavouriteType.Manga, response.User);
            GetFavouritesEmbed(results, addedValues, removedValues, combinedResponse.Characters, Wrapper.Models.Enums.FavouriteType.Characters,
                response.User);
            GetFavouritesEmbed(results, addedValues, removedValues, combinedResponse.Staff, Wrapper.Models.Enums.FavouriteType.Staff, response.User);
            GetFavouritesEmbed(results, addedValues, removedValues, combinedResponse.Studios, Wrapper.Models.Enums.FavouriteType.Studios,
                response.User);
            results.Sort((b1, b2) => b1.Color.Value.Value.CompareTo(b2.Color.Value.Value));
            return results;
        }
    }
}