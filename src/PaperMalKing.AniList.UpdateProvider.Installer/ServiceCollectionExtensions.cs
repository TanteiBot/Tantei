// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using System.Text.Json;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PaperMalKing.AniList.Wrapper;
using PaperMalKing.AniList.Wrapper.Abstractions;
using PaperMalKing.Common.RateLimiters;
using PaperMalKing.Database.Models.AniList;
using PaperMalKing.UpdatesProviders.Base.Features;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;
using Polly;

namespace PaperMalKing.AniList.UpdateProvider.Installer;

public static class ServiceCollectionExtensions
{
	public static void AddAniList(this IServiceCollection serviceCollection, IConfiguration configuration)
	{
		if (configuration.GetSection(AniListOptions.AniList).GetValue<int>(nameof(AniListOptions.DelayBetweenChecksInMilliseconds)) < 0)
		{
			return;
		}

		serviceCollection.AddOptions<AniListOptions>().BindConfiguration(AniListOptions.AniList).ValidateDataAnnotations().ValidateOnStart();
		const int rpm = 29;

		serviceCollection.AddHttpClient(ProviderConstants.Name).ConfigurePrimaryHttpMessageHandler(static () => new SocketsHttpHandler
		{
			PooledConnectionLifetime = TimeSpan.FromMinutes(30),
		})
		// https://github.com/TanteiBot/Tantei/issues/870
		.AddResilienceHandler("anilist", static builder => builder.AddRateLimiter(RateLimiterFactory.Create<AniListClient>(new(rpm, TimeSpan.FromMinutes(1)))));
		serviceCollection.AddSingleton<IAniListClient, AniListClient>(static provider =>
		{
			var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
			var httpClient = httpClientFactory.CreateClient(ProviderConstants.Name);
			httpClient.Timeout = TimeSpan.FromSeconds(200);
			var logger = provider.GetRequiredService<ILogger<AniListClient>>();
			var options = new GraphQLHttpClientOptions
			{
				EndPoint = new(ClientConstants.BaseUrl),
			};
			var gqlc = new GraphQLHttpClient(options, new SystemTextJsonSerializer(new JsonSerializerOptions(JsonSerializerDefaults.Web)), httpClient);

			return new(gqlc, logger);
		});
		serviceCollection.AddSingleton<BaseUserFeaturesService<AniListUser, AniListUserFeatures>, AniListUserFeaturesService>();
		serviceCollection.AddSingleton<AniListUserService>();

		serviceCollection.AddSingleton<AniListUpdateProvider>();
		serviceCollection.AddSingleton<BaseUpdateProvider>(static f => f.GetRequiredService<AniListUpdateProvider>());
		serviceCollection.AddHostedService(static f => f.GetRequiredService<AniListUpdateProvider>());
	}
}