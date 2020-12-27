using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace PaperMalKing.Common.RateLimiter
{
	public sealed class RateLimiterHttpMessageHandler<T> : DelegatingHandler, IDisposable
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
			await this.RateLimiter.TickAsync();
			return await base.SendAsync(request, cancellationToken);
		}

		public new void Dispose()
		{
			base.Dispose();
			if (this.RateLimiter is IDisposable disposable)
				disposable.Dispose();
		}
	}
}