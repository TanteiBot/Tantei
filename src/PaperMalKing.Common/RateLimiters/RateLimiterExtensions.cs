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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaperMalKing.Common.Options;

namespace PaperMalKing.Common.RateLimiters;

public static class RateLimiterExtensions
{
	public static RateLimiterHttpMessageHandler<T> ToHttpMessageHandler<T>(this IRateLimiter<T> rateLimiter) =>
		new(rateLimiter);

	public static IRateLimiter<T> ToRateLimiter<T>(this RateLimit rateLimit) =>
		RateLimiterFactory.Create<T>(rateLimit);

	public static IRateLimiter<T> ConfigurationLambda<TO, T>(IServiceProvider servicesProvider)
		where TO : class, IRateLimitOptions<T>
	{
		var logger = servicesProvider.GetRequiredService<ILogger<IRateLimiter<T>>>();
		var options = servicesProvider.GetRequiredService<IOptions<TO>>();
		return options.Value.ToRateLimiter(logger);
	}
}