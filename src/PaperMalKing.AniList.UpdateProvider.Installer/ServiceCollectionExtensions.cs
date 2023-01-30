using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.SystemTextJson;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PaperMalKing.AniList.Wrapper;
using PaperMalKing.AniList.Wrapper.Abstractions;
using PaperMalKing.AniList.Wrapper.Json;
using PaperMalKing.Database.Models.AniList;
using PaperMalKing.UpdatesProviders.Base.Features;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;

namespace PaperMalKing.AniList.UpdateProvider.Installer;

public static class ServiceCollectionExtensions
{
	public static void AddAniList(this IServiceCollection serviceCollection, IConfiguration configuration)
	{
		serviceCollection.AddOptions<AniListOptions>().Bind(configuration.GetSection(AniListOptions.AniList));

		serviceCollection.AddHttpClient(ProviderConstants.NAME).ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler()
		{
			PooledConnectionLifetime = TimeSpan.FromMinutes(30),
		}).AddHttpMessageHandler(provider =>
		{
			var rlLogger = provider.GetRequiredService<ILogger<HeaderBasedRateLimitMessageHandler>>();
			var rl = new HeaderBasedRateLimitMessageHandler(rlLogger);
			return rl;
		});
		serviceCollection.AddSingleton<IAniListClient, AniListClient>(provider =>
		{
			var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
			var httpClient = httpClientFactory.CreateClient(PaperMalKing.AniList.UpdateProvider.ProviderConstants.NAME);
			httpClient.Timeout = TimeSpan.FromSeconds(200);
			var logger = provider.GetRequiredService<ILogger<AniListClient>>();
			var options = new GraphQLHttpClientOptions()
			{
				EndPoint = new Uri(ClientConstants.BASE_URL)
			};
			var gqlc = new GraphQLHttpClient(options, new SystemTextJsonSerializer(new JsonSerializerOptions(JsonSerializerDefaults.Web)
			{
				Converters =
				{
					new JsonStringEnumConverter(new JsonUpperPolicyCase())
				}
			}), httpClient);

			return new(gqlc, logger);
		});
		serviceCollection.AddSingleton<BaseUserFeaturesService<AniListUser, AniListUserFeatures>, AniListUserFeaturesService>();
		serviceCollection.AddSingleton<AniListUserService>();
		serviceCollection.AddSingleton<IUpdateProvider, AniListUpdateProvider>();
	}
}