using HtmlAgilityPack;

namespace PaperMalKing.MyAnimeList.Wrapper.Parsers
{
	internal static class CommentsParser
	{
		internal static string Parse(HtmlNode node)
		{
			var text = node.SelectSingleNode("//title").InnerText;
			return text.Substring(0, text.LastIndexOf('\''));
		}
	}
}