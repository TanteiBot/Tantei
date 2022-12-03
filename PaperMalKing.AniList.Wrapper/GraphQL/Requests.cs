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

using GraphQL;
using PaperMalKing.AniList.Wrapper.Models;

namespace PaperMalKing.AniList.Wrapper.GraphQL
{
    internal static class Requests
    {
        public static GraphQLRequest GetUserInitialInfoByUsernameRequest(string username, byte favouritePage) =>
            new(Queries.GetUserInitialInfoByUsernameQuery, new {username = username, favouritePage = favouritePage});

        public static GraphQLRequest CheckForUpdatesRequest(ulong userId, byte page, long activityTimeStamp, ushort perChunk, ushort chunk,
                                                            RequestOptions options) =>
            new(UpdateCheckQueryBuilder.Build(options),
                new {userId = userId, page = page, activitiesFilterTimeStamp = activityTimeStamp, perChunk = perChunk, chunk = chunk});

        public static GraphQLRequest FavouritesInfoRequest(byte page, ulong[] animeIds, ulong[] mangaIds, ulong[] charIds, ulong[] staffIds,
            ulong[] studioIds, RequestOptions options) =>
            new(FavouritesInfoQueryBuilder.Build(options),
                new {page = page, animeIds = animeIds, mangaIds = mangaIds, charIds = charIds, staffIds = staffIds, studioIds = studioIds});
    }
}