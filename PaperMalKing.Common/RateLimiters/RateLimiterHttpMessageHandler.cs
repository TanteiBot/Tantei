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

using System.Diagnostics;
using System.Net.Http;
using System.Threading;
using System.Threading.RateLimiting;
using System.Threading.Tasks;

namespace PaperMalKing.Common.RateLimiters
{
	public sealed class RateLimiterHttpMessageHandler : DelegatingHandler
	{
		public RateLimiter RateLimiter { get; }

		internal RateLimiterHttpMessageHandler(RateLimiter rateLimiter)
		{
			this.RateLimiter = rateLimiter;
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				using (var rateLimitLease = await this.RateLimiter.AcquireAsync(1, cancellationToken).ConfigureAwait(false))
				{
					if (rateLimitLease.IsAcquired)
					{
						return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
					}
				}
			}

			cancellationToken.ThrowIfCancellationRequested();
			throw new UnreachableException();
		}

		protected override void Dispose(bool disposing)
		{
			RateLimiter.Dispose();
			base.Dispose(disposing);
		}
	}
}