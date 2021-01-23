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
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using Humanizer;
using PaperMalKing.AniList.UpdateProvider.CombinedResponses;
using PaperMalKing.AniList.Wrapper;
using PaperMalKing.AniList.Wrapper.Models;
using PaperMalKing.AniList.Wrapper.Models.Enums;
using PaperMalKing.Database.Models.AniList;

namespace PaperMalKing.AniList.UpdateProvider
{
    internal static class Extensions
    {
        private static readonly Dictionary<MediaListStatus, DiscordColor> Colors = new()
        {
            {MediaListStatus.PAUSED, Constants.AniListPeach},
            {MediaListStatus.CURRENT, Constants.AniListBlue},
            {MediaListStatus.REPEATING, Constants.AniListBlue},
            {MediaListStatus.DROPPED, Constants.AniListRed},
            {MediaListStatus.PLANNING, Constants.AniListOrange},
            {MediaListStatus.COMPLETED, Constants.AniListGreen}
        };

        public static async Task<CombinedRecentUpdatesResponse> GetAllRecentUserUpdatesAsync(this AniListClient client, AniListUser user,
            CancellationToken cancellationToken = default)
        {
            const ushort initialPerChunkValue = 50;
            const ushort extendedPerChunkValue = 500;
            var hasNextPage = true;
            var perChunk = initialPerChunkValue;
            ushort chunk = 1;
            var result = new CombinedRecentUpdatesResponse();
            for (byte page = 1; hasNextPage; page++)
            {
                var response = await client.CheckForUpdatesAsync(user.Id, page, user.LastActivityTimestamp, perChunk, chunk, cancellationToken);
                result.Add(response);
                hasNextPage = response.HasNextPage;
                if (perChunk == initialPerChunkValue)
                    perChunk = extendedPerChunkValue;
                else if (perChunk == extendedPerChunkValue) chunk++;
            }

            return result;
        }

        public static async Task<CombinedInitialInfoResponse> GetCompleteUserInitialInfoAsync(this AniListClient client, string username,
            CancellationToken cancellationToken = default)
        {
            var hasNextPage = true;
            var result = new CombinedInitialInfoResponse();
            for (byte page = 1; hasNextPage; page++)
            {
                var response = await client.GetInitialUserInfoAsync(username, page, cancellationToken);
                result.Add(response.User);
                hasNextPage = response.User.Favourites.HasNextPage;
            }

            return result;
        }

        public static string GetEmbedFormat(this Media media)
        {
            switch (media.CountryOfOrigin)
            {
                case "CN":
                    switch (media.Format)
                    {
                        case MediaFormat.TV:
                        case MediaFormat.TV_SHORT:
                        case MediaFormat.MOVIE:
                        case MediaFormat.SPECIAL:
                        case MediaFormat.OVA:
                        case MediaFormat.ONA:
                            return "Donghua";
                        case MediaFormat.MANGA:
                        case MediaFormat.ONE_SHOT:
                            return "Manhua";
                        default:
                            return media.Format.ToString().ToLowerInvariant().Humanize(LetterCasing.Sentence);
                    }
                case "KR":
                    switch (media.Format)
                    {
                        case MediaFormat.MANGA:
                        case MediaFormat.ONE_SHOT:
                            return "Manhwa";
                        default:
                            return media.Format.ToString().ToLowerInvariant().Humanize(LetterCasing.Sentence);
                    }
                default:
                    return media.Format.ToString().ToLowerInvariant().Humanize(LetterCasing.Sentence);
            }
        }

        public static DiscordEmbedBuilder WithMediaTitle(this DiscordEmbedBuilder eb, Media media, TitleLanguage titleLanguage)
        {
            var strings = new[]
            {
                media.Title.GetTitle(titleLanguage), $" ({media.GetEmbedFormat()})",
                $" [{media.Status.ToString().ToLowerInvariant().Humanize(LetterCasing.Sentence)}]"
            };
            var sb = new StringBuilder(256);
            foreach (var titlePart in strings)
            {
                if (sb.Length + titlePart.Length <= 256)
                    sb.Append(titlePart);
                else
                    break;
            }

            return eb.WithTitle(sb.ToString());
        }

        public static DiscordEmbedBuilder WithAniListAuthor(this DiscordEmbedBuilder embedBuilder, User user) =>
            embedBuilder.WithAuthor(user.Name, user.Url, user.Image.ImageUrl);

        public static DiscordEmbedBuilder ToDiscordEmbedBuilder(this Review review, User user)
        {
            return new DiscordEmbedBuilder().WithAniListAuthor(user).WithTitle(
                    $"New review on {review.Media.Title.GetTitle(user.Options.TitleLanguage)} ({review.Media.Format.Humanize(LetterCasing.Sentence)})")
                .WithThumbnail(review.Media.Image.ImageUrl).WithUrl(review.Url)
                .WithTimestamp(DateTimeOffset.FromUnixTimeSeconds(review.CreatedAtTimeStamp)).WithDescription(review.Summary);
        }

        public static DiscordEmbedBuilder ToDiscordEmbedBuilder(this ListActivity activity, MediaListEntry mediaListEntry, User user)
        {
            var isAnime = activity.Media.Type == ListType.ANIME;
            var isHiddenProgressPresent = !string.IsNullOrEmpty(activity.Progress) && (mediaListEntry.Status == MediaListStatus.PAUSED ||
                                                                                       mediaListEntry.Status == MediaListStatus.DROPPED ||
                                                                                       mediaListEntry.Status == MediaListStatus.COMPLETED);
            var desc = isHiddenProgressPresent switch
            {
                true => $"{(isAnime ? $"Watched episode" : $"Read chapter")} {activity.Progress} and {activity.Status.ToLowerInvariant()} it",
                _ => $"{activity.Status.Humanize(LetterCasing.Sentence)} {activity.Progress}"
            };
            var isAdvancedScoringEnabled =
                (isAnime
                    ? user.MediaListOptions.AnimeListOptions.IsAdvancedScoringEnabled
                    : user.MediaListOptions.MangaListOptions.IsAdvancedScoringEnabled) &&
                mediaListEntry.AdvancedScores?.Values.Any(s => s != 0) == true;
            var eb = new DiscordEmbedBuilder()
                .WithAniListAuthor(user)
                .WithTimestamp(DateTimeOffset.FromUnixTimeSeconds(activity.CreatedAtTimestamp))
                .WithUrl(activity.Media.Url)
                .WithMediaTitle(activity.Media, user.Options.TitleLanguage)
                .WithDescription(desc)
                .WithColor(Colors[mediaListEntry.Status])
                .WithThumbnail(activity.Media.Image.ImageUrl);
            var score = mediaListEntry.GetScore(user.MediaListOptions.ScoreFormat);
            if (!string.IsNullOrEmpty(score))
                eb.AddField("Score", score, true);

            if (isAdvancedScoringEnabled)
            {
                var sb = new StringBuilder();
                foreach (var (key, value) in mediaListEntry.AdvancedScores?.Where(e => e.Value != 0f) ??
                                             Enumerable.Empty<KeyValuePair<string, float>>())
                    sb.AppendLine($"{key}: {value:0.#}");
                eb.AddField("Advanced scoring", sb.ToString(), true);
            }

            eb.WithTotalSubEntries(activity.Media);
            if (!string.IsNullOrEmpty(mediaListEntry.Notes)) eb.AddField("Notes", mediaListEntry.Notes.Substring(0, 1023), true);
            if (mediaListEntry.Repeat != 0) eb.AddField($"{(isAnime ? "Rewatched times" : "Reread times")}", mediaListEntry.Repeat.ToString(), true);
            return eb;
        }

        public static DiscordEmbedBuilder WithTotalSubEntries(this DiscordEmbedBuilder eb, Media media)
        {
            if (!media.Episodes.HasValue && !media.Chapters.HasValue && !media.Volumes.HasValue) return eb;
            var fieldVal = new StringBuilder();
            if (media.Episodes.GetValueOrDefault() != 0) fieldVal.Append($"{media.Episodes!.Value.ToString()} ep.");
            if (media.Chapters.GetValueOrDefault() != 0) fieldVal.Append($"{media.Chapters!.Value.ToString()} ch,");
            if (media.Volumes.GetValueOrDefault() != 0) fieldVal.Append($"{media.Volumes!.Value.ToString()} v.");
            eb.AddField("Total", fieldVal.ToString(), true);

            return eb;
        }
    }
}