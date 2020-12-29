using System;

namespace PaperMalKing.Shikimori.Wrapper
{
	internal static class Utils
	{
		public static string GetImageUrl(string type, int id, string imageExt = "jpg", string size = "original") =>
			$"{Constants.BASE_URL}/system/{type}/{size}/{id.ToString()}.{imageExt}?{DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()}";

		public static string GetUrl(string type, int id) => $"{Constants.BASE_URL}/system/{type}/{id.ToString()}";
	}
}