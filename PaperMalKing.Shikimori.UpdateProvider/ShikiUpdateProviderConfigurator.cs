#region LICENSE
// PaperMalKing.
// Copyright (C) 2021 N0D4N
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
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaperMalKing.Common.RateLimiters;
using PaperMalKing.Database.Models.Shikimori;
using PaperMalKing.Shikimori.Wrapper;
using PaperMalKing.UpdatesProviders.Base;
using PaperMalKing.UpdatesProviders.Base.Features;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;

namespace PaperMalKing.Shikimori.UpdateProvider
{
	internal class ShikiUpdateProviderConfigurator : IUpdateProviderConfigurator<ShikiUpdateProvider>
	{
		/// <inheritdoc />
		[Obsolete("", true)]
		public void ConfigureNonStatic(IConfiguration configuration, IServiceCollection serviceCollection)
		{
			throw new NotSupportedException();
		}

		public static void Configure(IConfiguration configuration, IServiceCollection serviceCollection)
		{
			serviceCollection.AddOptions<ShikiOptions>().Bind(configuration.GetSection(Constants.NAME));

			var policy = HttpPolicyExtensions.HandleTransientHttpError().OrResult(message => message.StatusCode == HttpStatusCode.TooManyRequests)
											 .WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(10), 5));

			serviceCollection.AddHttpClient(Constants.NAME).AddPolicyHandler(policy).AddHttpMessageHandler(provider =>
			{
				var logger = provider.GetRequiredService<ILogger<IRateLimiter<ShikiClient>>>();
				var rl = new RateLimit(90, TimeSpan.FromMinutes(1.05d)); // 90rpm with .05 as inaccuracy
				return RateLimiterFactory.Create(rl, logger).ToHttpMessageHandler();
			}).AddHttpMessageHandler(provider =>
			{
				var logger = provider.GetRequiredService<ILogger<IRateLimiter<ShikiClient>>>();
				var rl = new RateLimit(5, TimeSpan.FromSeconds(1.05d)); //5rps with .05 as inaccuracy
				return RateLimiterFactory.Create(rl, logger).ToHttpMessageHandler();
			}).ConfigureHttpClient((provider, client) =>
			{
				client.DefaultRequestHeaders.UserAgent.Clear();
				client.DefaultRequestHeaders.UserAgent.ParseAdd($"{provider.GetRequiredService<IOptions<ShikiOptions>>().Value.ShikimoriAppName}");
				client.BaseAddress = new (Wrapper.Constants.BASE_URL);
			});
			serviceCollection.AddSingleton<ShikiClient>(provider =>
			{
				var factory = provider.GetRequiredService<IHttpClientFactory>();
				var logger = provider.GetRequiredService<ILogger<ShikiClient>>();
				return new(factory.CreateClient(Constants.NAME), logger);
			});
			serviceCollection.AddSingleton<IExecuteOnStartupService, ShikiExecuteOnStartupService>();
			serviceCollection.AddSingleton<IUserFeaturesService<ShikiUserFeatures>, ShikiUserFeaturesService>();
			serviceCollection.AddSingleton<ShikiUserService>();
			serviceCollection.AddSingleton<IUpdateProvider, ShikiUpdateProvider>();
		}
	}
}