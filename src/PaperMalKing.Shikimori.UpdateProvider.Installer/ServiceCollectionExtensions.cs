// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
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

namespace PaperMalKing.Shikimori.UpdateProvider.Installer;

public static class ServiceCollectionExtensions
{
	[SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "We only dispose when application closes")]
	public static void AddShikimori(this IServiceCollection serviceCollection, IConfiguration configuration)
	{
		if (configuration.GetSection(Constants.Name).GetValue<int>(nameof(ShikiOptions.DelayBetweenChecksInMilliseconds)) < 0)
		{
			return;
		}

		serviceCollection.AddOptions<ShikiOptions>().BindConfiguration(Constants.Name).ValidateDataAnnotations().ValidateOnStart();

		// https://shikimori.one/api/doc/1.0
		var rpmRl = RateLimiterFactory.Create<ShikiClient>(new(30, TimeSpan.FromMinutes(1, 25))); // 90rpm with .05 as inaccuracy
		var rpsRl = RateLimiterFactory.Create<ShikiClient>(new(2, TimeSpan.FromSeconds(1, 700))); // 5rps with .05 as inaccuracy

		serviceCollection.AddHttpClient(Constants.Name).ConfigureHttpClient(static (provider, client) =>
		{
			client.DefaultRequestHeaders.UserAgent.Clear();
			client.DefaultRequestHeaders.UserAgent.ParseAdd(provider.GetRequiredService<IOptions<ShikiOptions>>().Value.ShikimoriAppName);
		}).ConfigurePrimaryHttpMessageHandler(static () => new SocketsHttpHandler
		{
			PooledConnectionLifetime = TimeSpan.FromMinutes(15),
		}).AddResilienceHandler("shiki", builder =>
		{
			const int shikiHttpRetryAttempts = 5;

			builder.AddRetry(new HttpRetryStrategyOptions
			{
				MaxRetryAttempts = shikiHttpRetryAttempts,
			});

			builder.AddRateLimiter(rpmRl);

			builder.AddRateLimiter(rpsRl);
		});

		serviceCollection.AddSingleton<IShikiClient, ShikiClient>(static provider =>
		{
			var factory = provider.GetRequiredService<IHttpClientFactory>();
			var logger = provider.GetRequiredService<ILogger<ShikiClient>>();
			var httpClient = factory.CreateClient(Constants.Name);
			httpClient.BaseAddress = new(Wrapper.Abstractions.Constants.BaseUrl);

			var graphQlClient = new GraphQLHttpClient(Wrapper.Abstractions.Constants.GraphQlBaseUrl,
				new SystemTextJsonSerializer(new JsonSerializerOptions(JsonSerializerDefaults.Web)), factory.CreateClient(Constants.Name));

			return new(httpClient, logger, graphQlClient);
		});
		serviceCollection.AddSingleton<BaseUserFeaturesService<ShikiUser, ShikiUserFeatures>, ShikiUserFeaturesService>();
		serviceCollection.AddSingleton<ShikiUserService>();

		var achievementsPath = configuration.GetValue<string?>("Shikimori:PathToAchievementsJson") ?? "neko.json";
		var nekoFileJson = new NekoFileJson();
		if (File.Exists(achievementsPath))
		{
			using var stream = File.OpenRead(achievementsPath);

			nekoFileJson = JsonSerializer.Deserialize<NekoFileJson>(stream) ?? new NekoFileJson();
		}

		serviceCollection.AddSingleton<ShikiAchievementsService>((services) => new(nekoFileJson, services.GetRequiredService<ILogger<ShikiAchievementsService>>()));

		serviceCollection.AddSingleton<ShikiUpdateProvider>();
		serviceCollection.AddSingleton<BaseUpdateProvider>(static f => f.GetRequiredService<ShikiUpdateProvider>());
		serviceCollection.AddHostedService(static f => f.GetRequiredService<ShikiUpdateProvider>());
	}
}