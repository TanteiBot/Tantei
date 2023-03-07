// SPDX-License-Identifier: AGPL-3.0-or-later
// Copyright (C) 2021-2022 N0D4N

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PaperMalKing.AniList.UpdateProvider.Installer;
using PaperMalKing.MyAnimeList.UpdateProvider.Installer;
using PaperMalKing.Shikimori.UpdateProvider.Installer;
using PaperMalKing.UpdatesProviders.Base.UpdateProvider;

namespace PaperMalKing.Startup.Services;

internal sealed class UpdateProvidersConfigurationService
{
	private readonly Dictionary<string, IUpdateProvider> _providers = new(StringComparer.OrdinalIgnoreCase);

	public IReadOnlyDictionary<string, IUpdateProvider> Providers => this._providers;

	public UpdateProvidersConfigurationService(ILogger<UpdateProvidersConfigurationService> logger, IEnumerable<IUpdateProvider> updateProviders)
	{
		logger.LogTrace("Building {@UpdateProvidersConfigurationService}", typeof(UpdateProvidersConfigurationService));
		foreach (var updateProvider in updateProviders)
		{
			logger.LogDebug("Registering {@UpdateProvider} update provider", updateProvider);
			this._providers.Add(updateProvider.Name, updateProvider);
		}

		if (this._providers.Count == 0)
			logger.LogCritical("No update providers were registered");

		logger.LogTrace("Built {@UpdateProvidersConfigurationService}", typeof(UpdateProvidersConfigurationService));
	}

	public static void ConfigureProviders(IConfiguration configuration, IServiceCollection services)
	{
		services.AddAniList(configuration);
		services.AddMyAnimeList(configuration);
		services.AddShikimori(configuration);
	}
}