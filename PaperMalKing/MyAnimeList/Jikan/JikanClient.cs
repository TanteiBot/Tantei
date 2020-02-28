using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DSharpPlus;
using Newtonsoft.Json;
using PaperMalKing.MyAnimeList.Jikan.Data;
using PaperMalKing.MyAnimeList.Jikan.Data.Models;
using PaperMalKing.MyAnimeList.Jikan.Helpers;
using PaperMalKing.Utilities;

namespace PaperMalKing.MyAnimeList.Jikan
{
	public sealed class JikanClient
	{
		private readonly HttpClient _httpClient;

		private readonly bool _suppressExceptions;

        private long _lastRequestDate;

        private readonly LogDelegate Log;

        private const string LogName = "JikanClient";

        /// <summary>
		/// Constructor.
		/// </summary>
		public JikanClient(LogDelegate logDelegate)
        {
            this._lastRequestDate = DateTimeOffset.Now.Subtract(TimeSpan.FromSeconds(10)).ToUnixTimeMilliseconds();
            Log = logDelegate;
			this._suppressExceptions = true;
			this._httpClient = HttpProvider.GetHttpClient(true);
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="useHttps">Should client send SSL encrypted requests.</param>
		/// <param name="suppressExceptions">Should exception be thrown in case of failed request. If true, failed request return null.</param>
		public JikanClient(bool useHttps, LogDelegate logDelegate, bool suppressExceptions = true)
		{
            this._lastRequestDate = DateTimeOffset.Now.Subtract(TimeSpan.FromSeconds(10)).ToUnixTimeMilliseconds();
            Log = logDelegate;
            this._suppressExceptions = suppressExceptions;
			this._httpClient = HttpProvider.GetHttpClient(useHttps);
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="endpointUrl">Endpoint of the REST API.</param>
		/// <param name="suppressExceptions">Should exception be thrown in case of failed request. If true, failed request return null.</param>
		public JikanClient(string endpointUrl, LogDelegate logDelegate, bool suppressExceptions = true)
		{
            this._lastRequestDate = DateTimeOffset.Now.Subtract(TimeSpan.FromSeconds(10)).ToUnixTimeMilliseconds();
            Log = logDelegate;
            this._suppressExceptions = suppressExceptions;
			this._httpClient = HttpProvider.GetHttpClient(new Uri(endpointUrl));
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="endpointUrl">Endpoint of the REST API.</param>
		/// <param name="suppressExceptions">Should exception be thrown in case of failed request. If true, failed request return null.</param>
		public JikanClient(Uri endpointUrl, LogDelegate logDelegate, bool suppressExceptions = true)
		{
            this._lastRequestDate = DateTimeOffset.Now.Subtract(TimeSpan.FromSeconds(10)).ToUnixTimeMilliseconds();
            Log = logDelegate;
            this._suppressExceptions = suppressExceptions;
			this._httpClient = HttpProvider.GetHttpClient(endpointUrl);
		}

		private async Task<T> ExecuteGetRequestAsync<T>(string[] args) where T : BaseJikanRequest
		{
			T returnedObject = null;
			var requestUrl = string.Join("/", args);
            
            var timePassed = DateTimeOffset.Now.ToUnixTimeMilliseconds() - this._lastRequestDate;
            if (timePassed < 2000) //Delay between requests to Jikan should be 2 seconds
            {
                var delay = (int) (2000 - timePassed);
                this.Log(LogLevel.Debug, LogName, $"Waiting for {delay} ms before next request to Jikan",
                    DateTime.Now);
                await Task.Delay(delay);
            }


            try
            {
                bool tryAgain;
                do
                {
                    tryAgain = false;
                    using (var response = await this._httpClient.GetAsync(requestUrl))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            string json = await response.Content.ReadAsStringAsync();

                            returnedObject = JsonConvert.DeserializeObject<T>(json);
							if(!returnedObject.RequestCached)
								this._lastRequestDate = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                        }
                        else if (response.StatusCode == HttpStatusCode.TooManyRequests)
                        {
                            this.Log(LogLevel.Warning, LogName, "Got ratelimited for Jikan, waiting 10 s and retrying request again", DateTime.Now);
                            await Task.Delay(TimeSpan.FromSeconds(10));
                            tryAgain = true;
                        }
                        else if (!this._suppressExceptions)
                        {
                            throw new Exception($"Status code: '{response.StatusCode}'. Message: '{response.Content}'");
                        }
                    }
                } while (tryAgain);
            }
			catch (JsonSerializationException ex)
			{
				if (!this._suppressExceptions)
				{
					throw new Exception("Serialization failed" + ex.Message);
				}
			}
            return returnedObject;
		}

		/// <summary>
		/// Returns information about user's profile with given username.
		/// </summary>
		/// <param name="username">Username.</param>
		/// <returns>Information about user's profile with given username.</returns>
		public Task<UserProfile> GetUserProfileAsync(string username)
		{
			var endpointParts = new[] {EndpointCategories.User, username, "profile"};

			return this.ExecuteGetRequestAsync<UserProfile>(endpointParts);
		}

		/// <summary>
		/// Return anime with given MAL id.
		/// </summary>
		/// <param name="id">MAL id of anime.</param>
		/// <returns>Anime with given MAL id.</returns>
		public Task<Anime> GetAnimeAsync(long id)
		{
			var endpointParts = new[] { EndpointCategories.Anime, id.ToString() };
			return this.ExecuteGetRequestAsync<Anime>(endpointParts);
		}

		/// <summary>
		/// Returns entries on user's anime list.
		/// </summary>
		/// <param name="username">Username.</param>
		/// <param name="searchQuery">Query to search.</param>
		/// <returns>Entries on user's anime list.</returns>
		public Task<UserAnimeList> GetUserAnimeListAsync(string username, string searchQuery)
		{
			var query = string.Concat("animelist", $"?q={searchQuery}");
			var endpointParts = new[] { EndpointCategories.User, username, query };
			return this.ExecuteGetRequestAsync<UserAnimeList>(endpointParts);
		}

		/// <summary>
		/// Return manga with given MAL id.
		/// </summary>
		/// <param name="id">MAL id of manga.</param>
		/// <returns>Manga with given MAL id.</returns>
		public Task<Manga> GetMangaAsync(long id)
		{
			var endpointParts = new[] { EndpointCategories.Manga, id.ToString() };
			return this.ExecuteGetRequestAsync<Manga>(endpointParts);
		}

		/// <summary>
		/// Returns entries on user's manga list.
		/// </summary>
		/// <param name="username">Username.</param>
		/// <param name="searchQuery">Query to search.</param>
		/// <returns>Entries on user's manga list.</returns>
		public Task<UserMangaList> GetUserMangaList(string username, string searchQuery)
        {
            searchQuery = WebUtility.UrlEncode(searchQuery);
			var query = string.Concat("mangalist", $"?q={searchQuery}");
			var endpointParts = new[] { EndpointCategories.User, username, query };
			return this.ExecuteGetRequestAsync<UserMangaList>(endpointParts);
		}
	}
}
