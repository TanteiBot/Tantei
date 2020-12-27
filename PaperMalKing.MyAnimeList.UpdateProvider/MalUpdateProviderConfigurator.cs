using System;
using System.Net;
using System.Net.Http;
using Humanizer.Inflections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PaperMalKing.Common.RateLimiter;
using PaperMalKing.MyAnimeList.Wrapper;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;

namespace PaperMalKing.UpdatesProviders.MyAnimeList
{
	internal sealed class MalUpdateProviderConfigurator : IUpdateProviderConfigurator<MalUpdateProvider>
	{
		/// <inheritdoc />
		public void ConfigureNonStatic(IConfiguration configuration, IServiceCollection serviceCollection)
		{ }

		public static void Configure(IConfiguration configuration, IServiceCollection services)
		{
			Vocabularies.Default.AddPlural("ch", "chs.");
			Vocabularies.Default.AddPlural("v", "vs.");
			Vocabularies.Default.AddPlural("ep", "eps.");
			services.AddOptions<MalOptions>().Bind(configuration.GetSection(Constants.Name));
			services.AddSingleton(provider => RateLimiterExtensions.ConfigurationLambda<MalOptions,MyAnimeListClient>(provider));

			var retryPolicy = HttpPolicyExtensions.HandleTransientHttpError()
												  .OrResult(message =>
													  message.StatusCode == HttpStatusCode.TooManyRequests)
												  .WaitAndRetryAsync(
													  Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(10), 5));
			services.AddHttpClient(MalOptions.MyAnimeList)
					.AddPolicyHandler(retryPolicy)
					.ConfigurePrimaryHttpMessageHandler(_ => new HttpClientHandler
					{
						UseCookies = true,
						CookieContainer = new()
					}).AddHttpMessageHandler(provider =>
					{
						var rl = provider.GetRequiredService<IRateLimiter<MyAnimeListClient>>();
						return rl.ToHttpMessageHandler();
					});
			services.AddSingleton<MyAnimeListClient>(provider =>
			{
				var factory = provider.GetRequiredService<IHttpClientFactory>();
				var logger = provider.GetRequiredService<ILogger<MyAnimeListClient>>();
				return new(logger, factory.CreateClient(MalOptions.MyAnimeList));
			});
			services.AddSingleton<MalUserService>();
			services.AddSingleton<IUpdateProvider, MalUpdateProvider>();
		}
	}
}