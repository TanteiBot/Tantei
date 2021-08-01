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
using HtmlAgilityPack;
using PaperMalKing.Common.Enums;
using PaperMalKing.MyAnimeList.Wrapper.Models;
using PaperMalKing.MyAnimeList.Wrapper.Models.Progress;

namespace PaperMalKing.MyAnimeList.Wrapper.Parsers
{
	[SuppressMessage("Globalization", "CA1307")]
	internal static class LatestUpdatesParser
	{
		private static readonly char[] Numbers = {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9'};

		private const string BaseSelectorStart = "//div[contains(@class, 'updates ";
		private const string BaseSelectorEnd = "')]/div[1]";
		private const string AnimeSelector = BaseSelectorStart + "anime" + BaseSelectorEnd;
		private const string MangaSelector = BaseSelectorStart + "manga" + BaseSelectorEnd;

		internal static LatestInProfileUpdate? Parse(HtmlNode node, ListEntryType listEntryType)
		{
			var selector = listEntryType switch
			{
				ListEntryType.Anime => AnimeSelector,
				ListEntryType.Manga => MangaSelector,
				_                   => throw new ArgumentOutOfRangeException(nameof(listEntryType), listEntryType, null)
			};
			var dataNode = node.SelectSingleNode(selector);
			if (dataNode == null)
				return null;
			var hd = new HtmlDocument();
			hd.LoadHtml(dataNode.InnerHtml);
			dataNode = hd.DocumentNode;
			var link = dataNode.SelectSingleNode("//a").Attributes["href"].Value;
			var id = CommonParser.ExtractIdFromMalUrl(link);
			var dataText = dataNode.SelectSingleNode("//div[1]/div[2]").InnerText.Trim();
			var splitted = dataText.Split(Constants.DOT);
			var progressTypeValue = splitted[0].Trim();
			var scoreText = splitted[1].Trim();
			var index = progressTypeValue.IndexOfAny(Numbers);
			var progressValue = 0;
			var progress = GenericProgress.Unknown;
			if (index == -1)
			{
				progress = ProgressParser.Parse(progressTypeValue);
			}
			else
			{
				progress = ProgressParser.Parse(progressTypeValue.Substring(0, index - 1).Trim());
				var length = progressTypeValue.IndexOf('/') - index;
				if (length > 0)
					progressValue = int.Parse(progressTypeValue.Substring(index, length));
			}

			var score = 0;
			if (!scoreText.Contains('-'))
			{
				var scoreIndex = scoreText.LastIndexOf(' ');
				score = int.Parse(scoreText.Substring(scoreIndex));
			}

			return new()
			{
				Id = id,
				Progress = progress,
				ProgressValue = progressValue,
				Score = score
			};
		}
	}
}