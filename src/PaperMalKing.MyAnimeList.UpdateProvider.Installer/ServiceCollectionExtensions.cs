// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2023 N0D4N

using System;
using System.Net;
using System.Net.Http;
using JikanDotNet;
using JikanDotNet.Config;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaperMalKing.Common.RateLimiters;
using PaperMalKing.Database.Models.MyAnimeList;
using PaperMalKing.MyAnimeList.Wrapper;
using PaperMalKing.MyAnimeList.Wrapper.Abstractions;
using PaperMalKing.UpdatesProviders.Base.Features;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Installer;

public static class ServiceCollectionExtensions
{
	public static IServiceCollection AddMyAnimeList(this IServiceCollection serviceCollection, IConfiguration configuration)
	{
		serviceCollection.AddOptions<MalOptions>().Bind(configuration.GetSection(Constants.Name));
		serviceCollection.AddSingleton<RateLimiter<IMyAnimeListClient>>(RateLimiterExtensions.ConfigurationLambda<MalOptions, IMyAnimeListClient>);

		var retryPolicy = HttpPolicyExtensions.HandleTransientHttpError().OrResult(message => message.StatusCode == HttpStatusCode.TooManyRequests)
											  .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(10), 5));
		serviceCollection.AddHttpClient(Constants.UnOfficialApiHttpClientName).AddPolicyHandler(retryPolicy)
						 .ConfigurePrimaryHttpMessageHandler(_ => HttpClientHandlerFactory()).AddHttpMessageHandler(GetRateLimiterHandler)
						 .ConfigureHttpClient(client =>
						 {
							 client.DefaultRequestHeaders.UserAgent.Clear();
							 client.DefaultRequestHeaders.UserAgent.ParseAdd(
								 "Mozilla/5.0 (Macintosh; Intel Mac OS X 13_5) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36");
						 });
		serviceCollection.AddHttpClient(Constants.OfficialApiHttpClientName).AddPolicyHandler(retryPolicy)
						 .ConfigurePrimaryHttpMessageHandler(_ => HttpClientHandlerFactory()).AddHttpMessageHandler(GetRateLimiterHandler)
						 .ConfigureHttpClient((provider, client) =>
						 {
							 var options = provider.GetRequiredService<IOptions<MalOptions>>().Value;
							 client.DefaultRequestHeaders.Add(Constants.OfficialApiHeaderName, options.ClientId);
						 });
		serviceCollection.AddHttpClient(Constants.JikanHttpClientName).AddPolicyHandler(retryPolicy)
						 .ConfigurePrimaryHttpMessageHandler(_ => HttpClientHandlerFactory()).AddHttpMessageHandler(_ =>
						 {
							 var rl = new RateLimit(60, TimeSpan.FromMinutes(1.2d)); // 60rpm with 0.2 as inaccuracy
							 return RateLimiterFactory.Create<IJikan>(rl).ToHttpMessageHandler();
						 }).AddHttpMessageHandler(_ =>
						 {
							 var rl = new RateLimit(3, TimeSpan.FromSeconds(1.5)); // 3rps with 0.5 as inaccuracy
							 return RateLimiterFactory.Create<IJikan>(rl).ToHttpMessageHandler();
						 })
						 .ConfigureHttpClient(client => client.BaseAddress = new Uri("https://api.jikan.moe/v4/"));
		serviceCollection.AddSingleton<IJikan>(provider => new Jikan(new JikanClientConfiguration()
		{
			SuppressException = false,
			LimiterConfigurations = TaskLimiterConfiguration.None // We use System.Threading.RateLimiting
		}, provider.GetRequiredService<IHttpClientFactory>().CreateClient(Constants.JikanHttpClientName)));
		serviceCollection.AddSingleton<IMyAnimeListClient, MyAnimeListClient>(provider =>
		{
			var factory = provider.GetRequiredService<IHttpClientFactory>();
			var logger = provider.GetRequiredService<ILogger<MyAnimeListClient>>();
			var jikan = provider.GetRequiredService<IJikan>();
			return new(logger, unofficialApiHttpClient: factory.CreateClient(Constants.UnOfficialApiHttpClientName),
				officialApiHttpClient: factory.CreateClient(Constants.OfficialApiHttpClientName), jikanClient: jikan);
		});
		serviceCollection.AddSingleton<BaseUserFeaturesService<MalUser, MalUserFeatures>, MalUserFeaturesService>();
		serviceCollection.AddSingleton<MalUserService>();
		serviceCollection.AddSingleton<IUpdateProvider, MalUpdateProvider>();
		return serviceCollection;
	}

	private static SocketsHttpHandler HttpClientHandlerFactory() => new()
	{
		UseCookies = true,
		CookieContainer = new(),
		PooledConnectionLifetime = TimeSpan.FromMinutes(15)
	};

	private static RateLimiterHttpMessageHandler GetRateLimiterHandler(IServiceProvider provider) =>
		provider.GetRequiredService<RateLimiter<IMyAnimeListClient>>().ToHttpMessageHandler();
}