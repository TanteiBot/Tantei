using System;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaperMalKing.Common.RateLimiter;
using PaperMalKing.Shikimori.Wrapper;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;

namespace PaperMalKing.Shikimori.UpdateProvider
{
	internal class ShikiUpdateProviderConfigurator : IUpdateProviderConfigurator<ShikiUpdateProvider>
	{
		/// <inheritdoc />
		public void ConfigureNonStatic(IConfiguration configuration, IServiceCollection serviceCollection)
		{ }

		public static void Configure(IConfiguration configuration, IServiceCollection serviceCollection)
		{
			serviceCollection.AddOptions<ShikiOptions>().Bind(configuration.GetSection(Constants.NAME));

			var policy = HttpPolicyExtensions.HandleTransientHttpError().OrResult(message => message.StatusCode == HttpStatusCode.TooManyRequests)
											 .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(10), 5));

			serviceCollection.AddHttpClient(Constants.NAME).AddPolicyHandler(policy).AddHttpMessageHandler(provider =>
			{
				var logger = provider.GetRequiredService<ILogger<IRateLimiter<ShikiClient>>>();
				var rl = new RateLimit(90, TimeSpan.FromMinutes(1)); // 90rpm
				return RateLimiterFactory.Create(rl, logger).ToHttpMessageHandler();
			}).AddHttpMessageHandler(provider =>
			{
				var logger = provider.GetRequiredService<ILogger<IRateLimiter<ShikiClient>>>();
				var rl = new RateLimit(5, TimeSpan.FromSeconds(1)); //5rps
				return RateLimiterFactory.Create(rl, logger).ToHttpMessageHandler();
			}).ConfigureHttpClient((provider, client) =>
			{
				client.DefaultRequestHeaders.UserAgent.Clear();
				client.DefaultRequestHeaders.UserAgent.ParseAdd($"{provider.GetRequiredService<IOptions<ShikiOptions>>().Value.ShikimoriAppName}");
			});
			serviceCollection.AddSingleton<ShikiClient>(provider =>
			{
				var factory = provider.GetRequiredService<IHttpClientFactory>();
				var logger = provider.GetRequiredService<ILogger<ShikiClient>>();
				return new(factory.CreateClient(Constants.NAME), logger);
			});
			serviceCollection.AddSingleton<ShikiUserService>();
			serviceCollection.AddSingleton<IUpdateProvider, ShikiUpdateProvider>();
		}
	}
}