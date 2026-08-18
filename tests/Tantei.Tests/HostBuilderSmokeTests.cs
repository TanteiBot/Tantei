// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using PaperMalKing.Startup;
using PaperMalKing.Startup.Web;

namespace Tantei.Tests;

public sealed class HostBuilderSmokeTests
{
	private const string DisabledDelay = "-1";

	[Test]
	public async Task HostBuilds()
	{
		using var host = Host.CreateDefaultBuilder()
			.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>(StringComparer.Ordinal)
			{
				["ConnectionStrings:Default"] = "Data Source=:memory:",
				["Discord:Token"] = "smoke-test-token",
				["Discord:ClientId"] = "smoke-test-client-id",
				["Discord:ClientSecret"] = "smoke-test-client-secret",
				["Discord:Activities:0:ActivityType"] = "Playing",
				["Discord:Activities:0:PresenceText"] = "smoke test",
				["Discord:Activities:0:TimeToBeDisplayedInMilliseconds"] = "1000",
				["Discord:Activities:0:Status"] = "Online",
				["Seq:IsEnabled"] = "false",
				["Seq:IngestionUrl"] = "http://localhost",
				["Seq:ApiKey"] = "smoke-test",
				["Otlp:IsEnabled"] = "false",
				["Otlp:IngestionUrl"] = "http://localhost",
				["MyAnimeList:DelayBetweenChecksInMilliseconds"] = DisabledDelay,
				["Shikimori:DelayBetweenChecksInMilliseconds"] = DisabledDelay,
				["AniList:DelayBetweenChecksInMilliseconds"] = DisabledDelay,
				["Web:CookieLifetimeInDays"] = "30",
			}))
			.ConfigureServices((context, services) =>
			{
				services.AddWebAuthentication(context.Configuration);
				services.TryAddSingleton<EndpointDataSource>(new CompositeEndpointDataSource([]));
			})
			.UseDefaultServiceProvider(o =>
			{
				o.ValidateOnBuild = true;
				o.ValidateScopes = true;
			})
			.ConfigureBotServices()
			.ConfigureBotHost()
			.Build();

		await Assert.That(host).IsNotNull();
	}
}
