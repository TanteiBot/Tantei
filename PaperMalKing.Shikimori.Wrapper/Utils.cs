using System;

namespace PaperMalKing.Shikimori.Wrapper
{
	internal static class Utils
	{
		public static string GetImageUrl(string type, ulong id, string imageExt = "jpg", string size = "original") =>
			$"{Constants.BASE_URL}/system/{type}/{size}/{id.ToString()}.{imageExt}?{DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()}";

		public static string GetUrl(string type, ulong id) => $"{Constants.BASE_URL}/system/{type}/{id.ToString()}";
	}
}