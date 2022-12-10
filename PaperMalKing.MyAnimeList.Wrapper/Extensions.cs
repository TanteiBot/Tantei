// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N
using System;
using PaperMalKing.Common.Enums;
using PaperMalKing.MyAnimeList.Wrapper.Models;
using PaperMalKing.MyAnimeList.Wrapper.Models.List;
using PaperMalKing.MyAnimeList.Wrapper.Models.Progress;
using PaperMalKing.MyAnimeList.Wrapper.Models.Rss;
using PaperMalKing.MyAnimeList.Wrapper.Parsers;

namespace PaperMalKing.MyAnimeList.Wrapper;

internal static class Extensions
{
	internal static string ToLargeImage(this string value)
	{
		var s = value.Replace("/r/96x136", "", StringComparison.Ordinal).Replace("/r/80x120", "", StringComparison.Ordinal)
					 .Replace("/r/192x272", "", StringComparison.Ordinal);
		if (!s.Contains("l.jpg", StringComparison.Ordinal) && !s.Contains("characters", StringComparison.Ordinal))
			s = s.Replace(".jpg", "l.jpg", StringComparison.Ordinal);
		var i = s.IndexOf("?s", StringComparison.OrdinalIgnoreCase);
		return i <= 0 ? s : s.Remove(i);
	}

	internal static RecentUpdate ToRecentUpdate(this FeedItem feedItem, ListEntryType type)
	{
		var index = feedItem.Description.IndexOf('-', StringComparison.Ordinal);
		var progressText = feedItem.Description.Substring(0, index - 1).Trim();
		var progress = ProgressParser.Parse(progressText);
		var di = feedItem.Description.LastIndexOf("-", StringComparison.OrdinalIgnoreCase);
		var oi = feedItem.Description.LastIndexOf(" of", StringComparison.OrdinalIgnoreCase);
		var progressedSubEntries = int.Parse(feedItem.Description.Substring(di + 2, oi - di - 2));
		return new(type, CommonParser.ExtractIdFromMalUrl(feedItem.Link), feedItem.PublishingDateTimeOffset, progress, progressedSubEntries);
	}

	internal static (string inRssHash, string inProfileHash) GetHash(int id, int progressValue, GenericProgress progress, int score) =>
		new($"{id}{progressValue}", $"{progress}{score}");

	internal static (string inRssHash, string inProfileHash) GetHash(this IListEntry listEntry) =>
		GetHash(listEntry.Id, listEntry.ProgressedSubEntries, listEntry.UserProgress, listEntry.Score);
}