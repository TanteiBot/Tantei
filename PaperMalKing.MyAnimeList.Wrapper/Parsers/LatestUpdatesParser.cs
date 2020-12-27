﻿using System;
using HtmlAgilityPack;
using PaperMalKing.Common.Enums;
using PaperMalKing.MyAnimeList.Wrapper.Models;
using PaperMalKing.MyAnimeList.Wrapper.Models.Progress;

namespace PaperMalKing.MyAnimeList.Wrapper.Parsers
{
	internal static class LatestUpdatesParser
	{
		private static readonly char[] Numbers = {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9'};

		internal static LatestInProfileUpdate? Parse(HtmlNode node, ListEntryType listEntryType)
		{
			var type = listEntryType switch
			{
				ListEntryType.Anime => "anime",
				ListEntryType.Manga => "manga",
				_                   => throw new ArgumentOutOfRangeException(nameof(listEntryType), listEntryType, null)
			};
			var dataNode = node.SelectSingleNode($"//div[contains(@class, 'updates {type}')]/div[1]");
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