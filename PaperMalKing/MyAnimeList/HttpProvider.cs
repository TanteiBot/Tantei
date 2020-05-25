using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;

namespace PaperMalKing.MyAnimeList
{
	/// <summary>
	/// Provider class for static HttpClient.
	/// </summary>
	public static class HttpProvider
	{

		/// <summary>
		/// Get static HttpClient. Using custom, user defined Jikan REST endpoint.
		/// </summary>
		/// <param name="endpoint">Endpoint of the REST API.</param>
		/// <param name="timeout">Timespan before request times out</param>
		/// <returns>Static HttpClient.</returns>
		public static HttpClient GetHttpClientForJikan(Uri endpoint, TimeSpan timeout)
		{
			var client = new HttpClient
			{
				BaseAddress = endpoint,
				Timeout = timeout
			};
			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(
				new MediaTypeWithQualityHeaderValue("application/json"));
			client.DefaultRequestHeaders.UserAgent.Clear();
			client.DefaultRequestHeaders.UserAgent.ParseAdd("PaperMalKingBot");


			return client;
		}

		public static HttpClient GetHttpClientForMalFeedReader(TimeSpan timeout)
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
			client.DefaultRequestHeaders.UserAgent.TryParseAdd(
				"Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:76.0) Gecko/20100101 Firefox/76.0");
			return client;
		}
	}
}