#region LICENSE
// PaperMalKing.
// Copyright (C) 2021-2022 N0D4N
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

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Client.Http;
using Microsoft.Extensions.Logging;
using PaperMalKing.AniList.Wrapper.GraphQL;
using PaperMalKing.AniList.Wrapper.Models;
using PaperMalKing.AniList.Wrapper.Models.Responses;

namespace PaperMalKing.AniList.Wrapper
{
    public sealed class AniListClient
    {
        private readonly GraphQLHttpClient _client;
        private readonly ILogger<AniListClient> _logger;

        public AniListClient(GraphQLHttpClient client, ILogger<AniListClient> logger)
        {
            this._client = client;
            this._logger = logger;
        }

        internal async Task<InitialUserInfoResponse> GetInitialUserInfoAsync(string username, byte favouritesPage = 1,
            CancellationToken cancellationToken = default)
        {
            this._logger.LogDebug("Requesting initial info for {Username}, {Page}", username, favouritesPage);
            var request = Requests.GetUserInitialInfoByUsernameRequest(username, favouritesPage);
            var response = await this._client.SendQueryAsync<InitialUserInfoResponse>(request, cancellationToken).ConfigureAwait(false);
            return response.Data;
        }

        internal async Task<CheckForUpdatesResponse> CheckForUpdatesAsync(ulong userId, byte page, long activitiesTimeStamp, ushort perChunk,
            ushort chunk, RequestOptions options, CancellationToken cancellationToken)
        {
			cancellationToken.ThrowIfCancellationRequested();
            this._logger.LogDebug("Requesting updates check for {UserId}, {Page}", userId, page);
            var request = Requests.CheckForUpdatesRequest(userId, page, activitiesTimeStamp, perChunk, chunk, options);
            var response = await this._client.SendQueryAsync<CheckForUpdatesResponse>(request, cancellationToken).ConfigureAwait(false);
            return response.Data;
        }

        internal async Task<FavouritesResponse> FavouritesInfoAsync(byte page, ulong[] animeIds, ulong[] mangaIds, ulong[] charIds, ulong[] staffIds,
            ulong[] studioIds, RequestOptions options, CancellationToken cancellationToken = default)
        {
            if (!animeIds.Any() && !mangaIds.Any() && !charIds.Any() && !staffIds.Any() && !staffIds.Any() && !studioIds.Any())
                return FavouritesResponse.Empty;

            var request = Requests.FavouritesInfoRequest(page, animeIds, mangaIds, charIds, staffIds, studioIds, options);
            var response = await this._client.SendQueryAsync<FavouritesResponse>(request, cancellationToken).ConfigureAwait(false);
            return response.Data;
        }
    }
}