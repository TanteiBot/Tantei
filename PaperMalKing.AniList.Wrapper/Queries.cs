namespace PaperMalKing.AniList.Wrapper
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

        public const string CheckForUpdatesQuery = @"
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
            }
            AnimeList: MediaListCollection(userId: $userId, type: ANIME, sort: UPDATED_TIME_DESC, chunk: $chunk, perChunk: $perChunk) {
              lists {
                entries {
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
                }
              }
            }
            MangaList: MediaListCollection(userId: $userId, type: MANGA, sort: UPDATED_TIME_DESC, chunk: $chunk, perChunk: $perChunk) {
              lists {
                entries {
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
                }
              }
            }
            ActivitiesPage: Page(page: $page, perPage: 50) {
              pageInfo {
                hasNextPage
              }
              values: activities(userId: $userId, type: MEDIA_LIST, sort: ID_DESC, createdAt_greater: $activitiesFilterTimeStamp) {
                ... on ListActivity {
                  status
                  progress
                  createdAt
                  media {
                    id
                    title {
                      stylisedRomaji: romaji(stylised: true)
                      romaji(stylised: false)
                      stylisedEnglish: english(stylised: true)
                      english(stylised: false)
                      stylisedNative: native(stylised: true)
                      native(stylised: false)
                    }
                    type
                    siteUrl
                    format
                    countryOfOrigin
                    status(version: 2)
                    episodes
                    chapters
                    volumes
                    image: coverImage {
                      large: extraLarge
                    }
                  }
                }
              }
            }
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