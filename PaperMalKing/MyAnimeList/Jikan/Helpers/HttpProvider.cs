using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace PaperMalKing.MyAnimeList.Jikan.Helpers
{
	/// <summary>
	/// Provider class for static HttpClient.
	/// </summary>
	public static class HttpProvider
	{
		/// <summary>
		/// Endpoint for not SSL encrypted requests.
		/// </summary>
		private const string _httpEndpoint = "http://api.jikan.moe/v3/";

		/// <summary>
		/// Endpoint for SSL encrypted requests.
		/// </summary>
		private const string _httpsEndpoint = "https://api.jikan.moe/v3/";


		private static TimeSpan Timeout = TimeSpan.FromSeconds(45);

		/// <summary>
		/// Constructor.
		/// </summary>
		static HttpProvider()
		{
		}

		/// <summary>
		/// Get static HttpClient. Using default Jikan REST endpoint.
		/// </summary>
		/// <param name="useHttps">Define if request should be send to SSL encrypted endpoint.</param>
		/// <returns>Static HttpClient.</returns>
		public static HttpClient GetHttpClient(bool useHttps)
		{
			var endpoint = useHttps ? _httpsEndpoint : _httpEndpoint;
			var client = new HttpClient
			{
				BaseAddress = new Uri(endpoint),
				Timeout = Timeout
			};
			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(
				new MediaTypeWithQualityHeaderValue("application/json"));

			return client;
		}

		/// <summary>
		/// Get static HttpClient. Using custom, user defined Jikan REST endpoint.
		/// </summary>
		/// <param name="endpoint">Endpoint of the REST API.</param>
		/// <returns>Static HttpClient.</returns>
		public static HttpClient GetHttpClient(Uri endpoint)
		{
			var client = new HttpClient
			{
				BaseAddress = endpoint,
				Timeout = Timeout
			};
			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(
				new MediaTypeWithQualityHeaderValue("application/json"));

			return client;
		}
	}
}