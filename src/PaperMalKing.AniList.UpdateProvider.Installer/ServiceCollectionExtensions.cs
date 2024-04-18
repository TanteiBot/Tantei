// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2024 N0D4N

using System;
using System.Net.Http;
using System.Text.Json;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PaperMalKing.AniList.Wrapper;
using PaperMalKing.AniList.Wrapper.Abstractions;
using PaperMalKing.Database.Models.AniList;
using PaperMalKing.UpdatesProviders.Base.Features;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;

namespace PaperMalKing.AniList.UpdateProvider.Installer;

public static class ServiceCollectionExtensions
{
	public static void AddAniList(this IServiceCollection serviceCollection)
	{
		serviceCollection.AddOptions<AniListOptions>().BindConfiguration(AniListOptions.AniList).ValidateDataAnnotations().ValidateOnStart();

		serviceCollection.AddHttpClient(ProviderConstants.Name).ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
		{
			PooledConnectionLifetime = TimeSpan.FromMinutes(30),
		}).AddHttpMessageHandler(provider =>
		{
			var rlLogger = provider.GetRequiredService<ILogger<HeaderBasedRateLimitMessageHandler>>();
			return new HeaderBasedRateLimitMessageHandler(rlLogger);
		});
		serviceCollection.AddSingleton<IAniListClient, AniListClient>(provider =>
		{
			var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
			var httpClient = httpClientFactory.CreateClient(ProviderConstants.Name);
			httpClient.Timeout = TimeSpan.FromSeconds(200);
			var logger = provider.GetRequiredService<ILogger<AniListClient>>();
			var options = new GraphQLHttpClientOptions
			{
				EndPoint = new Uri(ClientConstants.BaseUrl),
			};
			var gqlc = new GraphQLHttpClient(options, new SystemTextJsonSerializer(new JsonSerializerOptions(JsonSerializerDefaults.Web)), httpClient);

			return new(gqlc, logger);
		});
		serviceCollection.AddSingleton<BaseUserFeaturesService<AniListUser, AniListUserFeatures>, AniListUserFeaturesService>();
		serviceCollection.AddSingleton<AniListUserService>();
		serviceCollection.AddSingleton<IUpdateProvider, AniListUpdateProvider>();
	}
}