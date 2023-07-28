// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaperMalKing.Common.RateLimiters;
using PaperMalKing.Database.Models.Shikimori;
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
	public static IServiceCollection AddShikimori(this IServiceCollection serviceCollection, IConfiguration configuration)
	{
		serviceCollection.AddOptions<ShikiOptions>().Bind(configuration.GetSection(Constants.NAME));

		var policy = HttpPolicyExtensions.HandleTransientHttpError().OrResult(message => message.StatusCode == HttpStatusCode.TooManyRequests)
										 .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(10), 5));

		serviceCollection.AddHttpClient(Constants.NAME).ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler()
		{
			PooledConnectionLifetime = TimeSpan.FromMinutes(15),
		}).AddPolicyHandler(policy).AddHttpMessageHandler(_ =>
		{
			var rl = new RateLimit(90, TimeSpan.FromMinutes(1.05d)); // 90rpm with .05 as inaccuracy
			return RateLimiterFactory.Create<ShikiClient>(rl).ToHttpMessageHandler();
		}).AddHttpMessageHandler(_ =>
		{
			var rl = new RateLimit(5, TimeSpan.FromSeconds(1.05d)); //5rps with .05 as inaccuracy
			return RateLimiterFactory.Create<ShikiClient>(rl).ToHttpMessageHandler();
		}).ConfigureHttpClient((provider, client) =>
		{
			client.DefaultRequestHeaders.UserAgent.Clear();
			client.DefaultRequestHeaders.UserAgent.ParseAdd($"{provider.GetRequiredService<IOptions<ShikiOptions>>().Value.ShikimoriAppName}");
			client.BaseAddress = new(Wrapper.Abstractions.Constants.BASE_URL);
		});
		serviceCollection.AddSingleton<IShikiClient, ShikiClient>(provider =>
		{
			var factory = provider.GetRequiredService<IHttpClientFactory>();
			var logger = provider.GetRequiredService<ILogger<ShikiClient>>();
			return new(factory.CreateClient(Constants.NAME), logger);
		});
		serviceCollection.AddSingleton<BaseUserFeaturesService<ShikiUser, ShikiUserFeatures>, ShikiUserFeaturesService>();
		serviceCollection.AddSingleton<ShikiUserService>();
		serviceCollection.AddSingleton<IUpdateProvider, ShikiUpdateProvider>();
		var path = configuration.GetValue<string>("Shikimori:PathToAchievementsJson") ?? "neko.json";
		 ShikiAchievementJsonItem[] achievements = Array.Empty<ShikiAchievementJsonItem>();
		 if (File.Exists(path))
		 {
		 	achievements = JsonSerializer.Deserialize<ShikiAchievementJsonItem[]>(File.ReadAllText(path))!;
		 }
		 serviceCollection.AddSingleton<ShikiAchievementsService>(_ => new(achievements));

		return serviceCollection;
	}
}