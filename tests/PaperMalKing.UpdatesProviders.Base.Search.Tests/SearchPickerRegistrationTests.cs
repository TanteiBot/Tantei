// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using DSharpPlus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace PaperMalKing.UpdatesProviders.Base.Search.Tests;

public sealed class SearchPickerRegistrationTests
{
	[Test]
	public async Task AddSearchInstantiatesAndActivatesOneSingletonHandler()
	{
		var services = new ServiceCollection();
		using var client = new DiscordClient(new DiscordConfiguration { Token = "test-token", });
		services.AddSingleton(client);
		services.AddSingleton(typeof(Microsoft.Extensions.Logging.ILogger<>), typeof(NullLogger<>));
		services.AddSearch();
		await using var provider = services.BuildServiceProvider();

		var startupHandler = provider.GetServices<IExecuteOnStartupService>().OfType<SearchPickerComponentHandler>().Single();
		var concreteHandler = provider.GetRequiredService<SearchPickerComponentHandler>();
		await startupHandler.ExecuteAsync(CancellationToken.None);
		await startupHandler.ExecuteAsync(CancellationToken.None);

		await Assert.That(startupHandler).IsSameReferenceAs(concreteHandler);
		await Assert.That(concreteHandler.ActivationCount).IsEqualTo(1);
		await Assert.That(provider.GetService<SearchPicker>()).IsNotNull();
		await Assert.That(provider.GetService<SearchOrchestrator>()).IsNotNull();
	}

	[Test]
	public async Task AddSearchIsIdempotentAcrossRepeatedCalls()
	{
		var services = new ServiceCollection();
		using var client = new DiscordClient(new DiscordConfiguration { Token = "test-token", });
		services.AddSingleton(client);
		services.AddSingleton(typeof(Microsoft.Extensions.Logging.ILogger<>), typeof(NullLogger<>));

		services.AddSearch();
		services.AddSearch();
		await using var provider = services.BuildServiceProvider();

		await Assert.That(provider.GetServices<IExecuteOnStartupService>().OfType<SearchPickerComponentHandler>().Count()).IsEqualTo(1);
	}
}
