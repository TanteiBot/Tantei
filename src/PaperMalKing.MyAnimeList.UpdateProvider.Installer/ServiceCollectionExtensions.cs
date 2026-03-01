// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using JikanDotNet;
using JikanDotNet.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaperMalKing.Common.RateLimiters;
using PaperMalKing.Database.Models.MyAnimeList;
using PaperMalKing.MyAnimeList.Wrapper;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions;
using PaperMalKing.UpdatesProviders.Base.Features;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;
using Polly;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Installer;

public static class ServiceCollectionExtensions
{
	public static void AddMyAnimeList(this IServiceCollection serviceCollection, IConfiguration configuration)
	{
		if (configuration.GetSection(Constants.Name).GetValue<int>(nameof(MalOptions.DelayBetweenChecksInMilliseconds)) < 0)
		{
			return;
		}

		const int malHttpRetries = 3;

		serviceCollection.AddOptions<MalOptions>().BindConfiguration(Constants.Name).ValidateDataAnnotations().ValidateOnStart();
		serviceCollection.AddSingleton(RateLimiterExtensions.ConfigurationLambda<MalOptions, IMyAnimeListClient>);

		serviceCollection.AddHttpClient(Constants.UnOfficialApiHttpClientName, static client =>
						{
							client.Timeout = TimeSpan.FromSeconds(120L);
							client.DefaultRequestHeaders.UserAgent.Clear();
							client.DefaultRequestHeaders.UserAgent.ParseAdd(Constants.UserAgent);
						}).ConfigurePrimaryHttpMessageHandler(static _ => HttpClientHandlerFactory()).AddResilienceHandler("parser-mal", static (builder, rbc) =>
						{
							builder.AddRetry(new HttpRetryStrategyOptions
							{
								MaxRetryAttempts = malHttpRetries,
							});

							var rateLimiter = rbc.ServiceProvider.GetRequiredService<RateLimiter<IMyAnimeListClient>>();
							builder.AddRateLimiter(rateLimiter);
						});
		serviceCollection.AddHttpClient(Constants.OfficialApiHttpClientName).ConfigurePrimaryHttpMessageHandler(static _ => HttpClientHandlerFactory())
						 .ConfigureHttpClient(static (provider, client) =>
						 {
							 var options = provider.GetRequiredService<IOptions<MalOptions>>().Value;
							 client.DefaultRequestHeaders.Add(Constants.OfficialApiHeaderName, options.ClientId);
						 }).AddResilienceHandler("official-mal", static (builder, rbc) =>
						 {
							 builder.AddRetry(new HttpRetryStrategyOptions
							 {
								 MaxRetryAttempts = malHttpRetries,
							 });

							 var rateLimiter = rbc.ServiceProvider.GetRequiredService<RateLimiter<IMyAnimeListClient>>();
							 builder.AddRateLimiter(rateLimiter);
						 });
		serviceCollection.AddHttpClient(Constants.JikanHttpClientName, static client => client.BaseAddress = new(Constants.JikanApiUrl))
						 .ConfigurePrimaryHttpMessageHandler(static _ => HttpClientHandlerFactory())
						 .AddResilienceHandler("jikan", static builder =>
						 {
							 // https://docs.api.jikan.moe/#section/Information/Rate-Limiting
							 var rpmRl = new RateLimitValue(50, TimeSpan.FromMinutes(1, 25)); // 60rpm with 0.2 as inaccuracy
							 builder.AddRateLimiter(RateLimiterFactory.Create<IJikan>(rpmRl));

							 var rpsRl = new RateLimitValue(2, TimeSpan.FromSeconds(1, 500)); // 3rps with 0.5 as inaccuracy
							 builder.AddRateLimiter(RateLimiterFactory.Create<IJikan>(rpsRl));
						 });

		serviceCollection.AddSingleton<IJikan>(static provider => new Jikan(
			new()
			{
				SuppressException = false,
				LimiterConfigurations = TaskLimiterConfiguration.None, // We use System.Threading.RateLimiting
			},
			provider.GetRequiredService<IHttpClientFactory>().CreateClient(Constants.JikanHttpClientName)));

		serviceCollection.AddSingleton<IMyAnimeListClient, MyAnimeListClient>(static provider =>
		{
			var factory = provider.GetRequiredService<IHttpClientFactory>();
			var logger = provider.GetRequiredService<ILogger<MyAnimeListClient>>();
			var jikan = provider.GetRequiredService<IJikan>();
			return new(logger, _unofficialApiHttpClient: factory.CreateClient(Constants.UnOfficialApiHttpClientName),
				_officialApiHttpClient: factory.CreateClient(Constants.OfficialApiHttpClientName), _jikanClient: jikan);
		});
		serviceCollection.AddSingleton<BaseUserFeaturesService<MalUser, MalUserFeatures>, MalUserFeaturesService>();
		serviceCollection.AddSingleton<MalUserService>();

		serviceCollection.AddSingleton<MalUpdateProvider>();
		serviceCollection.AddSingleton<BaseUpdateProvider>(static f => f.GetRequiredService<MalUpdateProvider>());
		serviceCollection.AddHostedService(static f => f.GetRequiredService<MalUpdateProvider>());
	}

	private static SocketsHttpHandler HttpClientHandlerFactory() => new()
	{
		UseCookies = true,
		CookieContainer = new(),
		PooledConnectionLifetime = TimeSpan.FromMinutes(15),
	};
}