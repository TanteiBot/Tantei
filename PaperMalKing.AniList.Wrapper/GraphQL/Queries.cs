// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

namespace PaperMalKing.AniList.Wrapper.GraphQL
{
	internal static class Queries
	{
		public const string GetUserInitialInfoByUsernameQuery = @"
          query ($username: String, $favouritePage: Int) {
            User(name: $username) {
              id
              favourites {
                anime(page: $favouritePage, perPage: 25) {
                  pageInfo {
                    hasNextPage
                  }
                  values: nodes {
                    id
                  }
                }
                manga(page: $favouritePage, perPage: 25) {
                  pageInfo {
                    hasNextPage
                  }
                  values: nodes {
                    id
                  }
                }
                characters(page: $favouritePage, perPage: 25) {
                  pageInfo {
                    hasNextPage
                  }
                  values: nodes {
                    id
                  }
                }
                staff(page: $favouritePage, perPage: 25) {
                  pageInfo {
                    hasNextPage
                  }
                  values: nodes {
                    id
                  }
                }
                studios(page: $favouritePage, perPage: 25) {
                  pageInfo {
                    hasNextPage
                  }
                  values: nodes {
                    id
                  }
                }
              }
            }
          }
          ";

		public const string FavouritesInfoQuery = @"
          query ($page: Int, $animeIds: [Int], $mangaIds: [Int], $charIds: [Int], $staffIds: [Int], $studioIds: [Int]) {
            Animes: Page(page: $page, perPage: 50) {
              pageInfo{
                hasNextPage
              }
              values: media(type: ANIME, id_in: $animeIds) {
                title {
                  stylisedRomaji: romaji(stylised: true)
                  romaji(stylised: false)
                  stylisedEnglish: english(stylised: true)
                  english(stylised: false)
                  stylisedNative: native(stylised: true)
                  native(stylised: false)
                }
                siteUrl
                countryOfOrigin
                type
                format
                id
                image: coverImage {
                  large: extraLarge
                }
              }
            }
            Mangas: Page(page: $page, perPage: 50) {
              pageInfo{
                hasNextPage
              }
              values: media(type: MANGA, id_in: $mangaIds) {
                title {
                  stylisedRomaji: romaji(stylised: true)
                  romaji(stylised: false)
                  stylisedEnglish: english(stylised: true)
                  english(stylised: false)
                  stylisedNative: native(stylised: true)
                  native(stylised: false)
                }
                countryOfOrigin
                type
                siteUrl
                format
                id
                image: coverImage {
                  large: extraLarge
                }
              }
            }
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
                media(sort: POPULARITY_DESC, page: 1, perPage: 1) {
                  values: nodes {
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
                  }
                }
              }
            }
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
                siteUrl
                image {
                  large
                }
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
                media(sort: POPULARITY_DESC, isMain: true, page: 1, perPage: 1) {
                  values: nodes {
                    title {
                      stylisedRomaji: romaji(stylised: true)
                      romaji(stylised: false)
                      stylisedEnglish: english(stylised: true)
                      english(stylised: false)
                      stylisedNative: native(stylised: true)
                      native(stylised: false)
                    }
                    siteUrl
                    image: coverImage {
                      large: extraLarge
                    }
                    format
                  }
                }
              }
            }
          }
          ";
	}
}