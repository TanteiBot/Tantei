using System;
using System.Net;
using System.Net.Http;

namespace PaperMalKing.Utilities
{
	/// <summary>
	/// Provider class for static HttpClient.
	/// </summary>
	internal static class HttpProvider
	{
		internal static HttpClient GetHttpClient(TimeSpan timeout)
		{
			var handler = new HttpClientHandler
			{
				UseCookies = true,
				CookieContainer = new CookieContainer()
			};
			
			var client = new HttpClient(handler)
			{
				Timeout = timeout
			};
			client.DefaultRequestHeaders.UserAgent.Clear();

			return client;
		}

		public static HttpClient GetHttpClientForMalFeedReader(TimeSpan timeout)
		{
			

			var client = new HttpClient()
			{
				Timeout = timeout
			};
			client.DefaultRequestHeaders.UserAgent.Clear();
			client.DefaultRequestHeaders.UserAgent.TryParseAdd(
				"Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:76.0) Gecko/20100101 Firefox/76.0");
			return client;
		}
	}
}