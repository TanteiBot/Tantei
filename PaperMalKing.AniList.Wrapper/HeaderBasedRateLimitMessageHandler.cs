#region LICENSE
// PaperMalKing.
// Copyright (C) 2021 N0D4N
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace PaperMalKing.AniList.Wrapper
{
	internal sealed class HeaderBasedRateLimitMessageHandler : DelegatingHandler
	{
		private readonly ILogger<HeaderBasedRateLimitMessageHandler> _logger;
		private const sbyte RateLimitMaxRequests = 90;
		private sbyte _rateLimitRemaining;
		private long _timestamp;
		private readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

		public HeaderBasedRateLimitMessageHandler(ILogger<HeaderBasedRateLimitMessageHandler> logger)
		{
			this._logger = logger;
			this._rateLimitRemaining = RateLimitMaxRequests;
			this._timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			await this._semaphoreSlim.WaitAsync(cancellationToken).ConfigureAwait(false);
			try
			{
				HttpResponseMessage response;
				do
				{
					var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
					if (now - this._timestamp >= 60)
					{
						this._logger.LogTrace("Resetting AniList ratelimiter");
						this._timestamp = now;
						this._rateLimitRemaining = RateLimitMaxRequests;
					}

					this._rateLimitRemaining--;
					if (this._rateLimitRemaining < 0)
					{
						var delay = this._timestamp + 60 - now;
						this._logger.LogDebug("AniList exceeded rate-limit waiting {Delay}", delay);
						await Task.Delay(TimeSpan.FromSeconds(Math.Min(delay, 60)), cancellationToken).ConfigureAwait(false);
						this._timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
						this._rateLimitRemaining = RateLimitMaxRequests;
					}

					response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);

					if (response.StatusCode == HttpStatusCode.TooManyRequests && response.Headers.RetryAfter?.Delta != null)
					{
						var delay = response.Headers.RetryAfter.Delta.Value.Add(TimeSpan.FromSeconds(1));
						this._logger.LogInformation("Got 429'd waiting {Delay}", delay);
						await Task.Delay(delay, cancellationToken).ConfigureAwait(false);
					}
					else
					{
						this._rateLimitRemaining = sbyte.Parse(response.Headers.GetValues("X-RateLimit-Remaining").First());
						this._logger.LogTrace("AniList rate limit remaining {RateLimitRemaining}", this._rateLimitRemaining);
					}
				} while (!cancellationToken.IsCancellationRequested && response.StatusCode == HttpStatusCode.TooManyRequests);

				return response;

			}
			finally
			{
				this._semaphoreSlim.Release();
			}
		}
	}
}