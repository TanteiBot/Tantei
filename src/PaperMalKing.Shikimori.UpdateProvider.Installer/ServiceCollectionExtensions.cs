// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
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
	public static IServiceCollection AddShikimori(this IServiceCollection serviceCollection, IConfiguration configuration)
	{
		serviceCollection.AddOptions<ShikiOptions>().Bind(configuration.GetSection(Constants.Name));

		var policy = HttpPolicyExtensions.HandleTransientHttpError().OrResult(message => message.StatusCode == HttpStatusCode.TooManyRequests)
										 .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(10), 5));

		serviceCollection.AddHttpClient(Constants.Name).ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
		{
			PooledConnectionLifetime = TimeSpan.FromMinutes(15),
		}).AddPolicyHandler(policy).AddHttpMessageHandler(_ =>
		{
			var rl = new RateLimitValue(90, TimeSpan.FromMinutes(1.05d)); // 90rpm with .05 as inaccuracy
			return RateLimiterFactory.Create<ShikiClient>(rl).ToHttpMessageHandler();
		}).AddHttpMessageHandler(_ =>
		{
			var rl = new RateLimitValue(5, TimeSpan.FromSeconds(1.05d)); // 5rps with .05 as inaccuracy
			return RateLimiterFactory.Create<ShikiClient>(rl).ToHttpMessageHandler();
		}).ConfigureHttpClient((provider, client) =>
		{
			client.DefaultRequestHeaders.UserAgent.Clear();
			client.DefaultRequestHeaders.UserAgent.ParseAdd($"{provider.GetRequiredService<IOptions<ShikiOptions>>().Value.ShikimoriAppName}");
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
		serviceCollection.AddSingleton<IUpdateProvider, ShikiUpdateProvider>();
		var path = configuration.GetValue<string>("Shikimori:PathToAchievementsJson") ?? "neko.json";
		var file = File.Exists(path)
			? JsonSerializer.Deserialize<NekoFileJson>(File.ReadAllText(path))!
			: new()
			{
				Achievements = [],
				HumanNames = new(0, StringComparer.Ordinal),
			};
		serviceCollection.AddSingleton<ShikiAchievementsService>(_ => new(file));

		return serviceCollection;
	}
}