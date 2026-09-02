// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaperMalKing.Common.RateLimiters;
using PaperMalKing.Database.Models.MyAnimeList;
using PaperMalKing.MyAnimeList.UpdateProvider.Search;
using PaperMalKing.MyAnimeList.Wrapper;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions;
using PaperMalKing.MyAnimeList.Wrapper.Tenrai;
using PaperMalKing.UpdatesProviders.Base;
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

		serviceCollection.TryAddSingleton(TimeProvider.System);
		serviceCollection.AddSingleton<TenraiEnrichmentTelemetry>();
		serviceCollection.AddSingleton<TenraiCooldown>();
		serviceCollection.AddSingleton<TenraiCircuit>();
		serviceCollection.AddSingleton<TenraiRateLimiter>();
		serviceCollection.AddMemoryCache();
		serviceCollection.AddSingleton<MalSearchPicker>();
		serviceCollection.AddSingleton<MalSearchService>();
		serviceCollection.AddSingleton<SearchPickerComponentHandler>();
		serviceCollection.AddSingleton<IExecuteOnStartupService>(static provider => provider.GetRequiredService<SearchPickerComponentHandler>());

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
		serviceCollection.AddHttpClient(Constants.TenraiHttpClientName, static client =>
						 {
							 client.BaseAddress = new(Constants.TenraiApiUrl);
							 client.Timeout = Timeout.InfiniteTimeSpan;
						 })
						 .ConfigurePrimaryHttpMessageHandler(static _ => new TenraiResponseBufferingHandler(HttpClientHandlerFactory()))
						 .AddHttpMessageHandler(static provider =>
							 new TenraiCircuitHandler(provider.GetRequiredService<TenraiCircuit>()))
						 .AddHttpMessageHandler(static provider =>
							 new TenraiCooldownHandler(
								 provider.GetRequiredService<TenraiCooldown>(),
								 provider.GetRequiredService<TenraiEnrichmentTelemetry>()))
						 .AddResilienceHandler("tenrai", static (builder, context) =>
						 {
							 var timeProvider = context.ServiceProvider.GetRequiredService<TimeProvider>();
							 var rateLimiter = context.ServiceProvider.GetRequiredService<TenraiRateLimiter>();
							 var cooldown = context.ServiceProvider.GetRequiredService<TenraiCooldown>();
							 var telemetry = context.ServiceProvider.GetRequiredService<TenraiEnrichmentTelemetry>();
							 TenraiResiliencePipeline.Configure(builder, timeProvider, rateLimiter, cooldown, telemetry);
						 });
		serviceCollection.AddSingleton<IMyAnimeListClient, MyAnimeListClient>(static provider =>
		{
			var factory = provider.GetRequiredService<IHttpClientFactory>();
			var logger = provider.GetRequiredService<ILogger<MyAnimeListClient>>();
			return new(logger, unofficialApiHttpClient: factory.CreateClient(Constants.UnOfficialApiHttpClientName),
				officialApiHttpClient: factory.CreateClient(Constants.OfficialApiHttpClientName),
				tenraiApiHttpClient: factory.CreateClient(Constants.TenraiHttpClientName),
				circuit: provider.GetRequiredService<TenraiCircuit>(),
				telemetry: provider.GetRequiredService<TenraiEnrichmentTelemetry>());
		});
		serviceCollection.AddSingleton<BaseUserFeaturesService<MalUser, MalUserFeatures>, MalUserFeaturesService>();
		serviceCollection.AddSingleton<MalUserService>();

		serviceCollection.AddSingleton<MalUpdateProvider>();
		serviceCollection.AddSingleton<BaseUpdateProvider>(static f => f.GetRequiredService<MalUpdateProvider>());
		serviceCollection.AddHostedService(static f => f.GetRequiredService<MalUpdateProvider>());
		serviceCollection.AddTransient<MalCommands>();
	}

	private static SocketsHttpHandler HttpClientHandlerFactory() => new()
	{
		UseCookies = true,
		CookieContainer = new(),
		PooledConnectionLifetime = TimeSpan.FromMinutes(15),
	};
}