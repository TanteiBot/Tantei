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

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace PaperMalKing.Common.RateLimiters;

public sealed class RateLimiter<T> : IRateLimiter<T>, IDisposable
{
	private readonly string _serviceName;
	public RateLimit RateLimit { get; }
	private ILogger<IRateLimiter<T>> Logger { get; }
	private long _lastUpdateTime;

	private long _availablePermits;
	private volatile SemaphoreSlim? _semaphoreSlim;

	internal RateLimiter(RateLimit rateLimit, ILogger<IRateLimiter<T>>? logger)
	{
		this._serviceName = $"{typeof(RateLimiter<T>).Name}<{typeof(T).Name}>";
		this.RateLimit = rateLimit;

		this.Logger = logger ?? NullLogger<RateLimiter<T>>.Instance;
		this._lastUpdateTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
		this._availablePermits = rateLimit.AmountOfRequests;
		this._semaphoreSlim = new(1, 1);
	}

	public async Task TickAsync(CancellationToken cancellationToken = default)
	{
		var slim = this._semaphoreSlim;
		if (slim != null)
		{
			await slim.WaitAsync(cancellationToken).ConfigureAwait(false);
			try
			{
				var nextRefillDateTime = this._lastUpdateTime + this.RateLimit.PeriodInMilliseconds;
				var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
				var arePermitsAvailable = this._availablePermits > 0;
				var isTooEarlyToRefill = now < nextRefillDateTime;
				if (isTooEarlyToRefill && !arePermitsAvailable)
				{
					var delay = nextRefillDateTime - now;
					this.Logger.LogDebug("[{ServiceName}] Waiting {Delay}ms", this._serviceName, delay.ToString());
					await Task.Delay((int)delay, cancellationToken).ConfigureAwait(false);
				}
				else if (isTooEarlyToRefill) // && arePermitsAvailable
				{
					this.Logger.LogInformation("[{ServiceName}] Passing", this._serviceName);
					this._availablePermits--;
					return;
				}

				this._lastUpdateTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
				this._availablePermits = this.RateLimit.AmountOfRequests - 1;
			}
			finally
			{
				slim.Release();
			}
		}
	}

	/// <inheritdoc />
	public override string ToString()
	{
		return $"[{this._serviceName}] with rate limits {this.RateLimit}";
	}

	/// <inheritdoc />
	public void Dispose()
	{
		var semaphore = this._semaphoreSlim;
		this._semaphoreSlim = null;
		semaphore?.Dispose();
	}
}