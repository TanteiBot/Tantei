// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaperMalKing.Common.RateLimiters;
using PaperMalKing.Database.Models.Shikimori;
using PaperMalKing.Shikimori.UpdateProvider.Achievements;
using PaperMalKing.Shikimori.Wrapper;
using PaperMalKing.Shikimori.Wrapper.Abstractions;
using PaperMalKing.UpdatesProviders.Base.Features;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;

namespace PaperMalKing.Shikimori.UpdateProvider.Installer;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddShikimori(this IServiceCollection serviceCollection)
	{
		serviceCollection.AddOptions<ShikiOptions>().BindConfiguration(Constants.Name).ValidateDataAnnotations().ValidateOnStart();

		var policy = HttpPolicyExtensions.HandleTransientHttpError().OrResult(message => message.StatusCode == HttpStatusCode.TooManyRequests)
										 .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(10), 5));

		serviceCollection.AddHttpClient(Constants.Name).ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
		{
			PooledConnectionLifetime = TimeSpan.FromMinutes(15),
		}).AddPolicyHandler(policy).AddHttpMessageHandler(_ =>
		{
			var rl = new RateLimitValue(50, TimeSpan.FromMinutes(1, 5)); // 90rpm with .05 as inaccuracy
			return RateLimiterFactory.Create<ShikiClient>(rl).ToHttpMessageHandler();
		}).AddHttpMessageHandler(_ =>
		{
			var rl = new RateLimitValue(3, TimeSpan.FromSeconds(1, 200)); // 5rps with .05 as inaccuracy
			return RateLimiterFactory.Create<ShikiClient>(rl).ToHttpMessageHandler();
		}).ConfigureHttpClient((provider, client) =>
		{
			client.DefaultRequestHeaders.UserAgent.Clear();
			client.DefaultRequestHeaders.UserAgent.ParseAdd(provider.GetRequiredService<IOptions<ShikiOptions>>().Value.ShikimoriAppName);
			client.BaseAddress = new(Wrapper.Abstractions.Constants.BaseUrl);
		});
		serviceCollection.AddSingleton<IShikiClient, ShikiClient>(provider =>
		{
			var factory = provider.GetRequiredService<IHttpClientFactory>();
			var logger = provider.GetRequiredService<ILogger<ShikiClient>>();
			return new(factory.CreateClient(Constants.Name), logger);
		});
		serviceCollection.AddSingleton<BaseUserFeaturesService<ShikiUser, ShikiUserFeatures>, ShikiUserFeaturesService>();
		serviceCollection.AddSingleton<ShikiUserService>();

		serviceCollection.AddOptions<NekoFileJson>().BindConfiguration("ShikimoriNeko").ValidateDataAnnotations().ValidateOnStart();
		serviceCollection.AddSingleton<ShikiAchievementsService>();

		serviceCollection.AddSingleton<ShikiUpdateProvider>();
		serviceCollection.AddSingleton<IUpdateProvider>(f => f.GetRequiredService<ShikiUpdateProvider>());
		serviceCollection.AddHostedService(f => f.GetRequiredService<ShikiUpdateProvider>());

		return serviceCollection;
	}
}