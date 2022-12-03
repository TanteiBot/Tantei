#region LICENSE
// PaperMalKing.
// Copyright (C) 2021-2022 N0D4N
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

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
using PaperMalKing.Database.Models.AniList;
using PaperMalKing.UpdatesProviders.Base;
using PaperMalKing.UpdatesProviders.Base.Features;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;

namespace PaperMalKing.AniList.UpdateProvider
{
    public sealed class AniListUpdateProviderConfigurator : IUpdateProviderConfigurator<AniListUpdateProvider>
    {
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
                httpClient.Timeout = TimeSpan.FromSeconds(200);
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
            serviceCollection.AddSingleton<IExecuteOnStartupService, AniListExecuteOnStartupService>();
            serviceCollection.AddSingleton<IUserFeaturesService<AniListUserFeatures>, AniListUserFeaturesService>();
            serviceCollection.AddSingleton<AniListUserService>();
            serviceCollection.AddSingleton<IUpdateProvider, AniListUpdateProvider>();
        }
    }
}