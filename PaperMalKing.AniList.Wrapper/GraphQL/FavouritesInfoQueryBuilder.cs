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

using System.Text;
using PaperMalKing.AniList.Wrapper.Models;

namespace PaperMalKing.AniList.Wrapper.GraphQL
{
	internal static class FavouritesInfoQueryBuilder
	{
		public static string Build(RequestOptions options)
		{
			var sb = new StringBuilder();
			sb.AppendLine(@" query ($page: Int, $animeIds: [Int], $mangaIds: [Int], $charIds: [Int], $staffIds: [Int], $studioIds: [Int]) {
                  Animes: Page(page: $page, perPage: 50) {
                    pageInfo{
                      hasNextPage
                    }
                    values: media(type: ANIME, id_in: $animeIds) {");
			Helpers.AppendMediaFields(sb, options);
			sb.AppendLine(@"}
                  }
                  Mangas: Page(page: $page, perPage: 50) {
                    pageInfo{
                      hasNextPage
                    }
                    values: media(type: MANGA, id_in: $mangaIds) {");
			Helpers.AppendMediaFields(sb, options);
			sb.AppendLine(@"}
                  }");

			sb.AppendLine(@"
                  Staff: Page(page: $page, perPage: 50) {
                    pageInfo{
                      hasNextPage
                    }
                    values: staff(id_in: $staffIds) {
                      name {
                        full
                        native
                      }
                      id
					  primaryOccupations,
					  staffMedia(sort:POPULARITY_DESC, page: 1, perPage: 1){");
			FillLesserMediaFields(sb);
			sb.Append(@"}
                      siteUrl
                      image {
                        large
                      }
                    ");
			if ((options & RequestOptions.MediaDescription) != 0)
				sb.AppendLine("description(asHtml: false)");

			sb.AppendLine(@"}
                  }");
			sb.AppendLine(@"
                  Characters: Page(page: $page, perPage: 50) {
                    pageInfo{
                      hasNextPage
                    }
                    values: characters(id_in: $charIds) {
                      name {
                        full
                        native
                      }
                      siteUrl
                      id
                      image {
                        large
                      }
                      media(sort: POPULARITY_DESC, page: 1, perPage: 1) {");
			FillLesserMediaFields(sb);
			sb.Append(@"}
                    }
                  }
                  Studios: Page(page: $page, perPage: 50) {
                    pageInfo{
                      hasNextPage
                    }
                    values: studios(id_in: $studioIds) {
                      name
                      siteUrl
                      id
                      media(sort: POPULARITY_DESC, isMain: true, page: 1, perPage: 1) {");

			FillLesserMediaFields(sb);
			sb.Append(@"}
                    }
                  }
              }");
			return sb.ToString();
		}

		private static void FillLesserMediaFields(StringBuilder sb)
		{
			sb.Append(@"""values: nodes {
                          title {
                            stylisedRomaji: romaji(stylised: true)
                            romaji(stylised: false)
                            stylisedEnglish: english(stylised: true)
                            english(stylised: false)
                            stylisedNative: native(stylised: true)
                            native(stylised: false)
                          }
                          siteUrl
                          format
                        }""");
		}
	}
}