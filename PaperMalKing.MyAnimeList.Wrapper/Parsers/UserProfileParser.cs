using HtmlAgilityPack;
using PaperMalKing.Common.Enums;
using PaperMalKing.MyAnimeList.Wrapper.Models;

namespace PaperMalKing.MyAnimeList.Wrapper.Parsers
{
	internal static partial class UserProfileParser
	{
		internal static User Parse(HtmlNode node)
		{
			var reportUrl = node.SelectSingleNode("//a[contains(@class, 'header-right')]").Attributes["href"].Value;
			var li = reportUrl.LastIndexOf('=');

			var id = int.Parse(reportUrl.Substring(li + 1));
			var url = node.SelectSingleNode("//meta[@property='og:url']").Attributes["content"].Value;
			var username = url.Substring(url.LastIndexOf('/') + 1);
			var favorites = FavoritesParser.Parse(node);
			//var rssNode = node.SelectSingleNode("//div[@class = 'user-profile-sns']");

			return new()
			{
				Favorites = favorites,
				Username = username,
				Id = id,
				LatestAnimeUpdate = LatestUpdatesParser.Parse(node, ListEntryType.Anime),
				LatestMangaUpdate = LatestUpdatesParser.Parse(node, ListEntryType.Manga)
			};
		}
	}
}