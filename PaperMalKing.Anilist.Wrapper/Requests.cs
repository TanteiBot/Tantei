using GraphQL;

namespace PaperMalKing.AniList.Wrapper
{
    internal static class Requests
    {
        public static GraphQLRequest GetUserInitialInfoByUsernameRequest(string username, byte favouritePage) =>
            new(Queries.GetUserInitialInfoByUsernameQuery, new {username = username, favouritePage = favouritePage});

        public static GraphQLRequest CheckForUpdatesRequest(ulong userId, byte page, long activityTimeStamp, ushort perChunk, ushort chunk) =>
            new(Queries.CheckForUpdatesQuery,
                new {userId = userId, page = page, activitiesFilterTimeStamp = activityTimeStamp, perChunk = perChunk, chunk = chunk});

        public static GraphQLRequest FavouritesInfoRequest(byte page, ulong[] animeIds, ulong[] mangaIds, ulong[] charIds, ulong[] staffIds,
            ulong[] studioIds) =>
            new(Queries.FavouritesInfoQuery,
                new {page = page, animeIds = animeIds, mangaIds = mangaIds, charIds = charIds, staffIds = staffIds, studioIds = studioIds});
    }
}