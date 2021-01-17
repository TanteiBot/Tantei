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
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;

namespace PaperMalKing.AniList.UpdateProvider
{
    internal class AniListUpdateProviderConfigurator : IUpdateProviderConfigurator<AniListUpdateProvider>
    {
        [Obsolete("", true)]
        public void ConfigureNonStatic(IConfiguration configuration, IServiceCollection serviceCollection)
        {
            throw new NotSupportedException();
        }

        public static void Configure(IConfiguration configuration, IServiceCollection serviceCollection)
        {
            serviceCollection.AddOptions<AniListOptions>().Bind(configuration.GetSection(AniListOptions.AniList));

            serviceCollection.AddHttpClient(Constants.NAME).AddHttpMessageHandler(provider =>
                {
                    var rlLogger = provider.GetRequiredService<ILogger<HeaderBasedRateLimitMessageHandler>>();
                    var rl = new HeaderBasedRateLimitMessageHandler(rlLogger);
                    return rl;
                });
            serviceCollection.AddSingleton<AniListClient>(provider =>
            {
                var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient(Constants.NAME);
                var logger = provider.GetRequiredService<ILogger<AniListClient>>();
                var options = new GraphQLHttpClientOptions()
                {
                    EndPoint = new Uri(Wrapper.Constants.BASE_URL)
                };
                var gqlc = new GraphQLHttpClient(options, new SystemTextJsonSerializer(new JsonSerializerOptions()
                {
                    Converters =
                    {
                        new JsonStringEnumConverter(new JsonUpperPolicyCase())
                    }
                }), httpClient);

                return new(gqlc, logger);
            });
            serviceCollection.AddSingleton<AniListUserService>();
            serviceCollection.AddSingleton<IUpdateProvider, AniListUpdateProvider>();
        }
    }
}