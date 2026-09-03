// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2026 N0D4N

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace PaperMalKing.UpdatesProviders.Base.Search;

internal static class ServiceCollectionExtensions
{
	public static IServiceCollection AddSearch(this IServiceCollection serviceCollection)
	{
		serviceCollection.AddMemoryCache();
		serviceCollection.TryAddSingleton(TimeProvider.System);
		serviceCollection.TryAddSingleton<SearchPicker>();
		serviceCollection.TryAddSingleton<SearchOrchestrator>();
		serviceCollection.TryAddSingleton<SearchPickerComponentHandler>();
		serviceCollection.TryAddEnumerable(
			ServiceDescriptor.Singleton<IExecuteOnStartupService, SearchPickerComponentHandler>(
				static provider => provider.GetRequiredService<SearchPickerComponentHandler>()));
		return serviceCollection;
	}
}
