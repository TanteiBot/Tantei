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
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PaperMalKing.Common.RateLimiters
{
	public sealed class RateLimiterHttpMessageHandler<T> : DelegatingHandler
	{
		private IRateLimiter<T> _rateLimiter;
		private readonly object _lock = new();

		public IRateLimiter<T> RateLimiter
		{
			get => this._rateLimiter;
			set
			{
				var old = this.RateLimiter;
				lock (this._lock)
				{
					this._rateLimiter = value;
				}

				if (old is IDisposable disposable)
					disposable.Dispose();
			}
		}

		internal RateLimiterHttpMessageHandler(IRateLimiter<T> rateLimiter)
		{
			this._rateLimiter = rateLimiter;
		}

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
																	 CancellationToken cancellationToken)
		{
			await this.RateLimiter.TickAsync(cancellationToken).ConfigureAwait(false);
			return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
		}

		public new void Dispose()
		{
			base.Dispose();
			if (this.RateLimiter is IDisposable disposable)
				disposable.Dispose();
		}
	}
}