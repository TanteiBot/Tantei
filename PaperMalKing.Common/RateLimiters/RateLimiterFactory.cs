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
using System.Threading.RateLimiting;

namespace PaperMalKing.Common.RateLimiters
{
	public static class RateLimiterFactory
	{
		public static RateLimiter<T> Create<T>(RateLimit rateLimit)
		{
			if (rateLimit == null)
				throw new ArgumentNullException(nameof(rateLimit));
			if (rateLimit.AmountOfRequests == 0 || rateLimit.PeriodInMilliseconds == 0)
				return new RateLimiter<T>(NullRateLimiter.Instance);

			return new RateLimiter<T>(new FixedWindowRateLimiter(new FixedWindowRateLimiterOptions()
			{
				Window = TimeSpan.FromMilliseconds(rateLimit.PeriodInMilliseconds),
				AutoReplenishment = true,
				PermitLimit = rateLimit.AmountOfRequests,
				QueueLimit = 100,
				QueueProcessingOrder = QueueProcessingOrder.OldestFirst
			}));
			// return new RateLimiter<T>(rateLimit, logger);
		}
	}
}