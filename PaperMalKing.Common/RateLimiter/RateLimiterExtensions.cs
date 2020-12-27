using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaperMalKing.Common.Options;

namespace PaperMalKing.Common.RateLimiter
{
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
}