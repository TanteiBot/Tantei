// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaperMalKing.Common.RateLimiters;
using PaperMalKing.Database.Models.MyAnimeList;
using PaperMalKing.MyAnimeList.Wrapper;
using PaperMalKing.UpdatesProviders.Base.Features;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;

namespace PaperMalKing.UpdatesProviders.MyAnimeList;

public sealed class MalUpdateProviderConfigurator : IUpdateProviderConfigurator<MalUpdateProvider>
{
	public static void Configure(IConfiguration configuration, IServiceCollection serviceCollection)
	{
		serviceCollection.AddOptions<MalOptions>().Bind(configuration.GetSection(Constants.Name));
		serviceCollection.AddSingleton<RateLimiter<MyAnimeListClient>>(RateLimiterExtensions.ConfigurationLambda<MalOptions, MyAnimeListClient>);

		var retryPolicy = HttpPolicyExtensions.HandleTransientHttpError().OrResult(message => message.StatusCode == HttpStatusCode.TooManyRequests)
											  .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(10), 5));
		serviceCollection.AddHttpClient(Constants.UnOfficialApiHttpClientName).AddPolicyHandler(retryPolicy)
						 .ConfigurePrimaryHttpMessageHandler(_ => HttpClientHandlerFactory()).AddHttpMessageHandler(GetRateLimiterHandler)
						 .ConfigureHttpClient(client =>
						 {
							 client.DefaultRequestHeaders.UserAgent.Clear();
							 client.DefaultRequestHeaders.UserAgent.ParseAdd(
								 "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:76.0) Gecko/20100101 Firefox/76.0");
						 });
		serviceCollection.AddHttpClient(Constants.OfficialApiHttpClientName).AddPolicyHandler(retryPolicy)
						 .ConfigurePrimaryHttpMessageHandler(_ => HttpClientHandlerFactory()).AddHttpMessageHandler(GetRateLimiterHandler)
						 .ConfigureHttpClient((provider, client) =>
						 {
							 var options = provider.GetRequiredService<IOptions<MalOptions>>().Value;
							 client.DefaultRequestHeaders.Add(Constants.OfficialApiHeaderName, options.ClientId);
						 });
		serviceCollection.AddSingleton<MyAnimeListClient>(provider =>
		{
			var factory = provider.GetRequiredService<IHttpClientFactory>();
			var logger = provider.GetRequiredService<ILogger<MyAnimeListClient>>();
			return new(logger, unofficialApiHttpClient: factory.CreateClient(Constants.UnOfficialApiHttpClientName),
				officialApiHttpClient: factory.CreateClient(Constants.OfficialApiHttpClientName));
		});
		serviceCollection.AddSingleton<BaseUserFeaturesService<MalUser,MalUserFeatures>, MalUserFeaturesService>();
		serviceCollection.AddSingleton<MalUserService>();
		serviceCollection.AddSingleton<IUpdateProvider, MalUpdateProvider>();
	}

	private static SocketsHttpHandler HttpClientHandlerFactory() => new()
	{
		UseCookies = true,
		CookieContainer = new(),
		PooledConnectionLifetime = TimeSpan.FromMinutes(15)
	};

	private static RateLimiterHttpMessageHandler GetRateLimiterHandler(IServiceProvider provider) =>
		provider.GetRequiredService<RateLimiter<MyAnimeListClient>>().ToHttpMessageHandler();
}