using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GraphQL.Client.Http;
using Microsoft.Extensions.Logging;
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
            var response = await this._client.SendQueryAsync<InitialUserInfoResponse>(request, cancellationToken);
            return response.Data;
        }

        internal async Task<CheckForUpdatesResponse> CheckForUpdatesAsync(ulong userId, byte page, long activitiesTimeStamp, ushort perChunk,
            ushort chunk, CancellationToken cancellationToken = default)
        {
            this._logger.LogDebug("Requesting updates check for {UserId}, {Page}", userId, page);
            var request = Requests.CheckForUpdatesRequest(userId, page, activitiesTimeStamp, perChunk, chunk);
            var response = await this._client.SendQueryAsync<CheckForUpdatesResponse>(request, cancellationToken);
            return response.Data;
        }

        internal async Task<FavouritesResponse> FavouritesInfoAsync(byte page, ulong[] animeIds, ulong[] mangaIds, ulong[] charIds, ulong[] staffIds,
            ulong[] studioIds, CancellationToken cancellationToken = default)
        {
            if (!animeIds.Any() && !mangaIds.Any() && !charIds.Any() && !staffIds.Any() && !staffIds.Any() && !studioIds.Any())
                return FavouritesResponse.Empty;
            
            var request = Requests.FavouritesInfoRequest(page, animeIds, mangaIds, charIds, staffIds, studioIds);
            var response = await this._client.SendQueryAsync<FavouritesResponse>(request, cancellationToken);
            return response.Data;
        }
    }
}