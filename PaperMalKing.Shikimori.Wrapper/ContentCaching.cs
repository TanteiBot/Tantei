using System.Collections.Concurrent;
using System.Net.Http;

namespace PaperMalKing.Shikimori.Wrapper
{
	internal static class ContentCaching
	{
		public static readonly MultipartFormDataContent UserCachedContent = new()
		{
			{new StringContent("1"), "is_nickname"}
		};

		public static readonly ConcurrentDictionary<(uint, byte), MultipartFormDataContent> UserHistoryCachedContent = new();

		public static readonly ConcurrentDictionary<(uint, string), MultipartFormDataContent> UserListContent = new();
	}
}