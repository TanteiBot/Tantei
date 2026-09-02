// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using PaperMalKing.Common.RateLimiters;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions;

namespace PaperMalKing.MyAnimeList.Wrapper.Tenrai;

public static class TenraiEnrichmentServiceCollectionExtensions
{
	public static IServiceCollection AddTenraiEnrichment(this IServiceCollection serviceCollection)
	{
		ArgumentNullException.ThrowIfNull(serviceCollection);
		serviceCollection.TryAddSingleton(TimeProvider.System);
		serviceCollection.AddSingleton<TenraiCircuit>();
		serviceCollection.AddSingleton<TenraiCooldown>();
		serviceCollection.AddSingleton(static _ => TenraiResiliencePipeline.CreateRateLimiter());
		var httpClientBuilder = serviceCollection.AddHttpClient(TenraiConstants.HttpClientName, static client =>
						 {
							 client.BaseAddress = new(TenraiConstants.ApiUrl);
							 client.Timeout = Timeout.InfiniteTimeSpan;
						 })
						 .ConfigurePrimaryHttpMessageHandler(static _ => new SocketsHttpHandler
						 {
							 UseCookies = true,
							 CookieContainer = new(),
							 PooledConnectionLifetime = TimeSpan.FromMinutes(15),
						 })
						 .AddHttpMessageHandler(static _ => new TenraiAttemptHandler())
						 .AddHttpMessageHandler(static provider => new TenraiCircuitHandler(provider.GetRequiredService<TenraiCircuit>()))
						 .AddHttpMessageHandler(static provider => new TenraiCooldownHandler(provider.GetRequiredService<TenraiCooldown>()));
		_ = httpClientBuilder.AddResilienceHandler("tenrai", static (builder, context) =>
		{
			var timeProvider = context.ServiceProvider.GetRequiredService<TimeProvider>();
			var rateLimiter = context.ServiceProvider.GetRequiredService<RateLimiter<TenraiClient>>();
			var cooldown = context.ServiceProvider.GetRequiredService<TenraiCooldown>();
			TenraiResiliencePipeline.Configure(builder, timeProvider, rateLimiter, cooldown);
		});
		_ = httpClientBuilder.AddHttpMessageHandler(static () => new TenraiResponseBufferingHandler());
		serviceCollection.AddSingleton<IMyAnimeListEnrichment>(static provider => new TenraiEnrichment(
			provider.GetRequiredService<ILogger<TenraiEnrichment>>(),
			provider.GetRequiredService<IHttpClientFactory>().CreateClient(TenraiConstants.HttpClientName),
			provider.GetRequiredService<TenraiCircuit>()));
		return serviceCollection;
	}
}
