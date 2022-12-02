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
using System.Diagnostics.CodeAnalysis;
using PaperMalKing.Common.Enums;
using PaperMalKing.MyAnimeList.Wrapper.Models;
using PaperMalKing.MyAnimeList.Wrapper.Models.List;
using PaperMalKing.MyAnimeList.Wrapper.Models.Progress;
using PaperMalKing.MyAnimeList.Wrapper.Models.Rss;
using PaperMalKing.MyAnimeList.Wrapper.Parsers;

namespace PaperMalKing.MyAnimeList.Wrapper
{
	[SuppressMessage("Globalization", "CA1307")]
	internal static class Extensions
	{
		internal static string ToLargeImage(this string value)
		{
			var s = value.Replace("/r/96x136", "").Replace("/r/80x120", "").Replace("/r/192x272", "");
			if (!s.Contains("l.jpg") && !s.Contains("characters"))
				s = s.Replace(".jpg", "l.jpg");
			var i = s.IndexOf("?s", StringComparison.OrdinalIgnoreCase);
			return i <=0 ? s : s.Remove(i);
		}

		internal static RecentUpdate ToRecentUpdate(this FeedItem feedItem, ListEntryType type)
		{
			var index = feedItem.Description.IndexOf('-');
			var progressText = feedItem.Description.Substring(0, index - 1).Trim();
			var progress = ProgressParser.Parse(progressText);
			var di = feedItem.Description.LastIndexOf("-", StringComparison.OrdinalIgnoreCase);
			var oi = feedItem.Description.LastIndexOf(" of", StringComparison.OrdinalIgnoreCase);
			var progressedSubEntries = int.Parse(feedItem.Description.Substring(di + 2, oi - di - 2));
			return new(type, CommonParser.ExtractIdFromMalUrl(feedItem.Link), feedItem.PublishingDateTimeOffset, progress, progressedSubEntries);
		}

		internal static (string inRssHash, string inProfileHash) GetHash(int id, int progressValue, GenericProgress progress, int score) =>
			new($"{id.ToString()}{progressValue.ToString()}", $"{progress.ToString()}{score.ToString()}");

		internal static (string inRssHash, string inProfileHash) GetHash(this IListEntry listEntry) =>
			GetHash(listEntry.Id, listEntry.ProgressedSubEntries, listEntry.UserProgress, listEntry.Score);
	}
}