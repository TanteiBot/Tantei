using System.Text.RegularExpressions;

namespace PaperMalKing.MyAnimeList.Wrapper.Parsers
{
	internal static class CommonParser
	{
		private static readonly Regex IdFromUrlRegex = new(@"(?<=\/)(?<id>\d+)(?=\/)", RegexOptions.Compiled);

		internal static int ExtractIdFromMalUrl(string url) => int.Parse(IdFromUrlRegex.Match(url).Groups["id"].Value);
	}
}