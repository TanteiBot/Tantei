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

using System.Text;
using PaperMalKing.AniList.Wrapper.Models;

namespace PaperMalKing.AniList.Wrapper.GraphQL
{
	internal static class UpdateCheckQueryBuilder
	{
		private const string QueryStart = @"
          query ($userId: Int, $page: Int, $activitiesFilterTimeStamp: Int, $perChunk: Int, $chunk: Int) {
            User(id: $userId) {
              id
              name
              siteUrl
              image: avatar {
                large
              }
              options {
                titleLanguage
              }
              mediaListOptions {
                scoreFormat
                animeList {
                  advancedScoringEnabled
                }
                mangaList {
                  advancedScoringEnabled
                }
              }
          ";

		private const string FavouritesSubQuery = @"
              favourites {
                anime(page: $page, perPage: 25) {
                  pageInfo {
                    hasNextPage
                  }
                  values: nodes {
                    id
                  }
                }
                manga(page: $page, perPage: 25) {
                  pageInfo {
                    hasNextPage
                  }
                  values: nodes {
                    id
                  }
                }
                characters(page: $page, perPage: 25) {
                  pageInfo {
                    hasNextPage
                  }
                  values: nodes {
                    id
                  }
                }
                staff(page: $page, perPage: 25) {
                  pageInfo {
                    hasNextPage
                  }
                  values: nodes {
                    id
                  }
                }
                studios(page: $page, perPage: 25) {
                  pageInfo {
                    hasNextPage
                  }
                  values: nodes {
                    id
                  }
                }
              }
              ";

		private const string AnimeListSubQuery = @"
            AnimeList: MediaListCollection(userId: $userId, type: ANIME, sort: UPDATED_TIME_DESC, chunk: $chunk, perChunk: $perChunk) {{
              lists {{
                entries {{
                  id: mediaId
                  status
                  point100Score: score(format: POINT_100)
                  point10DecimalScore: score(format: POINT_10_DECIMAL)
                  point10Score: score(format: POINT_10)
                  point5Score: score(format: POINT_5)
                  point3Score: score(format: POINT_3)
                  repeat
                  notes
                  advancedScores
				  {0}
                }}
              }}
            }}
            ";

		private const string MangaListSubQuery = @"
            MangaList: MediaListCollection(userId: $userId, type: MANGA, sort: UPDATED_TIME_DESC, chunk: $chunk, perChunk: $perChunk) {{
              lists {{
                entries {{
                  id: mediaId
                  status
                  point100Score: score(format: POINT_100)
                  point10DecimalScore: score(format: POINT_10_DECIMAL)
                  point10Score: score(format: POINT_10)
                  point5Score: score(format: POINT_5)
                  point3Score: score(format: POINT_3)
                  repeat
                  notes
                  advancedScores
				  {0}
                }}
              }}
            }}
            ";

		private const string ReviewsSubQuery = @"
            ReviewsPage: Page(page: $page, perPage: 50) {
              pageInfo {
                hasNextPage
              }
              values: reviews(userId: $userId, sort: CREATED_AT_DESC) {
                createdAt
                siteUrl
                summary
                media {
                  title {
                    stylisedRomaji: romaji(stylised: true)
                    romaji(stylised: false)
                    stylisedEnglish: english(stylised: true)
                    english(stylised: false)
                    stylisedNative: native(stylised: true)
                    native(stylised: false)
                  }
                  format
                  image: coverImage {
                    large: extraLarge
                  }
                }
              }
            }
            ";

		public static string Build(RequestOptions options)
		{
			var hasAnime = (options & RequestOptions.AnimeList) != 0;
			var hasManga = (options & RequestOptions.MangaList) != 0;
			var hasFavs = (options & RequestOptions.Favourites) != 0;


			var hasReview = (options & RequestOptions.Reviews) != 0;
			var sb = new StringBuilder(QueryStart);
			if (hasFavs)
			{
				sb.AppendLine(FavouritesSubQuery);
			}
			sb.AppendLine("}");
			if (hasAnime)
			{
				sb.AppendLine(string.Format(AnimeListSubQuery,
					(options & RequestOptions.CustomLists) != 0 ? "customLists(asArray: true)" : string.Empty));
			}

			if (hasManga)
			{
				sb.AppendLine(string.Format(MangaListSubQuery, (options & RequestOptions.CustomLists) != 0 ? "customLists(asArray: true)" : string.Empty));
			}
			if (hasAnime || hasManga)
			{
				sb.AppendLine(@"ActivitiesPage: Page(page: $page, perPage: 50) {
              pageInfo {
                hasNextPage
              }");
				var type = hasAnime switch
				{
					true when hasManga => "MEDIA_LIST",
					true => "ANIME_LIST",
					_ => "MANGA_LIST"
				};
				sb.AppendLine($"values: activities(userId: $userId, type: {type}, sort: ID_DESC, createdAt_greater: $activitiesFilterTimeStamp) {{");
				sb.AppendLine(@"... on ListActivity {
                  status
                  progress
                  createdAt
                  media {
                    ");
				if (hasAnime)
					sb.AppendLine("episodes");
				if (hasManga)
					sb.AppendLine(@"chapters
                    volumes");
				Helpers.AppendMediaFields(sb, options);
				sb.AppendLine(@"}
                }
              }
            }");
			}
			if (hasReview)
			{
				sb.AppendLine(ReviewsSubQuery);
			}
			sb.AppendLine("}");
			return sb.ToString();
		}
	}
}