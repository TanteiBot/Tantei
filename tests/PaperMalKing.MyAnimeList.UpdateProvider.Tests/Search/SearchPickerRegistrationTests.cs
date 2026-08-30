// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using DSharpPlus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using PaperMalKing.MyAnimeList.UpdateProvider.Installer;
using PaperMalKing.MyAnimeList.UpdateProvider.Search;
using PaperMalKing.UpdatesProviders.Base;

namespace PaperMalKing.MyAnimeList.UpdateProvider.Tests.Search;

public sealed class SearchPickerRegistrationTests
{
	[Test]
	public async Task EnabledProviderInstantiatesAndActivatesOneSingletonHandler()
	{
		var services = new ServiceCollection();
		using var client = new DiscordClient(new DiscordConfiguration { Token = "test-token", });
		services.AddSingleton(client);
		services.AddSingleton(typeof(Microsoft.Extensions.Logging.ILogger<>), typeof(NullLogger<>));
		services.AddMyAnimeList(Configuration(delay: 0));
		await using var provider = services.BuildServiceProvider();

		var startupHandler = provider.GetServices<IExecuteOnStartupService>().OfType<SearchPickerComponentHandler>().Single();
		var concreteHandler = provider.GetRequiredService<SearchPickerComponentHandler>();
		await startupHandler.ExecuteAsync(CancellationToken.None);
		await startupHandler.ExecuteAsync(CancellationToken.None);

		await Assert.That(startupHandler).IsSameReferenceAs(concreteHandler);
		await Assert.That(concreteHandler.ActivationCount).IsEqualTo(1);
		await Assert.That(services.Any(static descriptor => descriptor.ServiceType == typeof(MalSearchService))).IsTrue();
		await Assert.That(services.Any(static descriptor => descriptor.ServiceType == typeof(MalCommands))).IsTrue();
	}

	[Test]
	public async Task DisabledProviderRegistersNoPickerHandler()
	{
		var services = new ServiceCollection();

		services.AddMyAnimeList(Configuration(delay: -1));

		await Assert.That(services.Any(static descriptor => descriptor.ServiceType == typeof(SearchPickerComponentHandler))).IsFalse();
		await Assert.That(services.Any(static descriptor => descriptor.ServiceType == typeof(IExecuteOnStartupService))).IsFalse();
		await Assert.That(services.Any(static descriptor => descriptor.ServiceType == typeof(MalSearchService))).IsFalse();
		await Assert.That(services.Any(static descriptor => descriptor.ServiceType == typeof(MalCommands))).IsFalse();
	}

	private static IConfiguration Configuration(int delay) => new ConfigurationBuilder()
		.AddInMemoryCollection(new Dictionary<string, string?>(StringComparer.Ordinal)
		{
			[$"MyAnimeList:{nameof(MalOptions.DelayBetweenChecksInMilliseconds)}"] = delay.ToString(System.Globalization.CultureInfo.InvariantCulture),
		})
		.Build();
}
